using Cantina.Controllers;
using Cantina.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис хранит список юзеров онлайн.
    /// </summary>
    public class OnlineUsersService
    {
        // сервисы
        private readonly IServiceProvider _services;
        private readonly ILogger<OnlineUsersService> _logger;
        private readonly IHubContext<MainHub, IChatClient> _chatHub;
        private readonly bool _isDevelopMode;
        private readonly MessageService _messageService;

        // список юзеров в онлайне
        private static Dictionary<int, OnlineSession> OnlineUsers = new Dictionary<int, OnlineSession>(30);
        // блокировщик для изменений списка юзеров
        private static object _locker = new object();

        public OnlineUsersService(IServiceProvider serviceProvider, ILogger<OnlineUsersService> logger,
            MessageService messageService, IHubContext<MainHub, IChatClient> hub, IWebHostEnvironment env)
        {
            _services = serviceProvider;
            _logger = logger;
            _chatHub = hub;
            _messageService = messageService;
            _isDevelopMode = env.IsDevelopment();
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public async Task AddUser(int userId, string connectionId, UserOnlineStatus onlineStatus = UserOnlineStatus.Online)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                using var scope = _services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var userProfile = userService.GetUserProfile(userId);
                lock (_locker)
                {
                    OnlineUsers.Add(userId, new OnlineSession(connectionId, userProfile, onlineStatus));
                }

                if (_isDevelopMode) _logger.LogInformation("User '{0}' connected to chat", userProfile.Name);

                // рассылаем сообщение о входе
                if(onlineStatus != UserOnlineStatus.Hidden)
                {
                    var enterMessage = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = userProfile.Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System,
                        Text = "В Кантину заходит <author />."
                    };
                    _messageService.AddMessage(enterMessage);
                    await _chatHub.Clients.All.ReceiveMessage(enterMessage);
                    await _chatHub.Clients.AllExcept(connectionId).AddUserToOnlineList(OnlineUsers[userId]);
                }

            }
            // если юзер в списке уже есть - меняем статус новый если нужно и добавляем соединение
            else
            {
                var statusUpdated = OnlineUsers[userId].Status != onlineStatus;
                lock (_locker)
                {
                    OnlineUsers[userId].AddConnection(connectionId);
                    OnlineUsers[userId].Status = onlineStatus;
                }
                if (onlineStatus == UserOnlineStatus.Hidden)
                {
                    var exitMessage = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = OnlineUsers[userId].Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System,
                        Text = "<author /> растворяется в воздухе...",
                    };
                    _messageService.AddMessage(exitMessage);
                    await _chatHub.Clients.All.ReceiveMessage(exitMessage);
                    await _chatHub.Clients.AllExcept(connectionId).RemoveUserFromOnlineList(userId);
                } else if(statusUpdated)
                {
                    await _chatHub.Clients.AllExcept(connectionId).AddUserToOnlineList(OnlineUsers[userId]);
                }

                if (_isDevelopMode) _logger.LogInformation("User '{0}' is online", OnlineUsers[userId].Name);
            }
        }

        /// <summary>
        /// Отключение клиента / юзера
        /// </summary>
        public void RemoveConnection(int userId, string connectionId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                lock (_locker)
                {
                    OnlineUsers[userId].RemoveConnection(connectionId);
                }
                // если у юзера не осталось активных клиентов - переводим его в статус Offline (если юзер был невидим - удаляем из памяти)
                if (OnlineUsers[userId].Connections == 0)
                {
                    if (OnlineUsers[userId].Status != UserOnlineStatus.Hidden)
                    {
                        OnlineUsers[userId].Status = UserOnlineStatus.Offline;
                        if (_isDevelopMode) _logger.LogInformation("User '{0}' is offline", OnlineUsers[userId].Name);
                    }
                    else
                    {
                        lock (_locker)
                        {
                            OnlineUsers.Remove(userId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет юзера из списка онлайна в случае, если у него не более одного активного соединения
        /// сохраняес в профиле время онлайна и записывает визит в историю дейстивий
        /// </summary>
        public async Task RemoveUser(int userId)
        {
            if (OnlineUsers.ContainsKey(userId) && OnlineUsers[userId].Connections <= 1)
            {

                var onlineTime = Convert.ToInt32((DateTime.UtcNow - OnlineUsers[userId].EnterTime).TotalMinutes); // время юзера в онлайне
                var profile = OnlineUsers[userId].GetProfile();
                profile.OnlineTime += onlineTime;
                using var scope = _services.CreateScope();
                var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();
                dataBase.Update<UserProfile>(profile);
                if (onlineTime > 3) dataBase.History.Add(new UserHistory            // "Визит" сохраняем в иторию только если онлайн больше 3х минут
                {
                    Date = DateTime.UtcNow,
                    Type = ActivityTypes.Visit,
                    UserID = profile.UserId,
                    Description = onlineTime.ToString()
                });
                try
                {
                    await dataBase.SaveChangesAsync();
                    if (_isDevelopMode) _logger.LogInformation("{0}'s profile has been successfully updated.", profile.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to database while saving profile");
                }

                // рассылаем уведомление о выходе из чата
                if(OnlineUsers[userId].Status != UserOnlineStatus.Hidden)
                {
                    var exitMessage = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = OnlineUsers[userId].Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System,
                        Text = "<author /> покидает Кантину.",
                    };
                    _messageService.AddMessage(exitMessage);
                    await _chatHub.Clients.All.ReceiveMessage(exitMessage);
                }
                await _chatHub.Clients.All.RemoveUserFromOnlineList(userId);
                if (_isDevelopMode) _logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, onlineTime);
                
                // удаляем юзера из памяти
                lock (_locker)
                {
                    OnlineUsers.Remove(userId);
                }

            }

        }

        /// <summary>
        /// Метод удаляет из списка всех юзеров со статусом offline
        /// и сохраняет обновлённое время онлайна в профиле
        /// а так же добавляет запись о визите в таблицу действий
        /// </summary>
        public async Task CheckUsersStatus(TimeSpan interval)
        {   
            List<UserProfile> updatedProfiles = new List<UserProfile>();
            List<UserHistory> historyData = new List<UserHistory>();
            var nowTime = DateTime.Now;
            foreach (var keyValues in OnlineUsers)
            {
                var userSession = keyValues.Value;
                if (userSession.Status == UserOnlineStatus.Offline && (nowTime - userSession.LastActivityTime) > interval)
                {
                    var profile = userSession.GetProfile();
                    var onlineTime = Convert.ToInt32((userSession.LastActivityTime - userSession.EnterTime).TotalMinutes);
                    profile.OnlineTime += onlineTime;
                    updatedProfiles.Add(profile);
                    historyData.Add(new UserHistory
                    {
                        Date = nowTime,
                        Type = ActivityTypes.Visit,
                        UserID = userSession.UserId,
                        Description = onlineTime.ToString()
                    });
                    lock (_locker)
                    {
                        OnlineUsers.Remove(userSession.UserId);
                    }
                    var message = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = profile.Name,
                        DateTime = nowTime,
                        Type = MessageTypes.System,
                        Text = "<author /> куда-то пропадает..."
                    };
                    _messageService.AddMessage(message);
                    await _chatHub.Clients.All.ReceiveMessage(message);
                    await _chatHub.Clients.All.RemoveUserFromOnlineList(profile.UserId);
                    if (_isDevelopMode) _logger.LogInformation("User '{0}' romoved from online users.", profile.Name, onlineTime);
                }
            }
            if (updatedProfiles.Count == 0 && historyData.Count == 0) return;
            using var scope = _services.CreateScope();
            var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();
            dataBase.UpdateRange(updatedProfiles);
            dataBase.AddRange(historyData);
            try
            {
                var changes = await dataBase.SaveChangesAsync();
                if (_isDevelopMode) _logger.LogInformation($"Updates {changes} fields in DataBase. Update profiles - {updatedProfiles.Count}, added visits to history - {historyData.Count}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to update database");
            }
        }


        /// <summary>
        /// Сессия конкретного юзера в онлайне или null, если юзера нет.
        /// </summary>
        public OnlineSession GetSessionInfo(int userId)
        {
            if (OnlineUsers.ContainsKey(userId)) return OnlineUsers[userId];
            else return null;
        }

        /// <summary>
        /// Возвращаем коллекцию юзеров онлайн
        /// </summary>
        public IEnumerable<OnlineSession> GetOnlineUsers()
        {
            return from keyValue in OnlineUsers
                   where keyValue.Value.Status != UserOnlineStatus.Hidden   // Исключаем невидимых.
                   select keyValue.Value;
        }


        public bool UpdateUserProfile(UserProfile profile)
        {
            if (OnlineUsers.ContainsKey(profile.UserId))
            {
                lock (_locker)
                {
                    OnlineUsers[profile.UserId].Profile = profile;
                }
                return true;
            }
            return false;
        }

    }
}