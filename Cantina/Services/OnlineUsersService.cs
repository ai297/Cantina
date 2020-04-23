using Cantina.Controllers;
using Cantina.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheOptions; // время хранения данных в кеше

        // список юзеров в онлайне
        private static Dictionary<int, OnlineSession> OnlineUsers = new Dictionary<int, OnlineSession>(30);
        // блокировщик для изменений списка юзеров
        private static object _locker = new object();

        public OnlineUsersService(IServiceProvider serviceProvider, ILogger<OnlineUsersService> logger,
            MessageService messageService, IHubContext<MainHub, IChatClient> hub, IWebHostEnvironment env, IMemoryCache cache, IOptions<IntevalsOptions> options)
        {
            _services = serviceProvider;
            _logger = logger;
            _chatHub = hub;
            _messageService = messageService;
            _isDevelopMode = env.IsDevelopment();
            _memoryCache = cache;
            _cacheOptions = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(options.Value.UserCacheTime) };
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
                        lock(_locker)
                        {
                            OnlineUsers[userId].Status = UserOnlineStatus.Offline;
                        }
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
                dataBase.UserProfiles.Update(profile);

                // запись в истории об изменении имени
                if (!OnlineUsers[userId].OriginalName.Equals(profile.Name)) dataBase.History.Add(new UserHistory
                {
                    Date = DateTime.UtcNow,
                    Type = ActivityTypes.ChangeName,
                    UserID = profile.UserId,
                    Description = $"{OnlineUsers[userId].OriginalName}>{profile.Name}"
                });

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
                    User user = null;
                    if(_memoryCache.TryGetValue<User>(profile.UserId, out user))
                    {
                        user.Profile = profile;
                        _memoryCache.Set<User>(user.Id, user, _cacheOptions);
                    }
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
                if (_isDevelopMode) _logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, (onlineTime > 0) ? onlineTime : 0);
                
                // удаляем юзера из памяти
                lock (_locker)
                {
                    OnlineUsers.Remove(userId);
                }

            }

        }

        /// <summary>
        /// Метод проверяет состояние списка юзеров онлайн
        /// помечает неактивных юзеров
        /// удаляет из списка всех юзеров со статусом offline
        /// и сохраняет обновлённое время онлайна в профиле
        /// а так же добавляет запись о визите в таблицу действий
        /// </summary>
        public async Task CheckUsersStatus(TimeSpan offlineInterval, TimeSpan inactivityTime)
        {
            var nowTime = DateTime.UtcNow;
            List<UserProfile> updatedProfiles = new List<UserProfile>();
            List<UserHistory> historyData = new List<UserHistory>();

            foreach (var keyValues in OnlineUsers)
            {
                var userSession = keyValues.Value;
                var profile = userSession.GetProfile();
                var onlineTime = Convert.ToInt32((userSession.LastActivityTime - userSession.EnterTime).TotalMinutes);

                // если юзер неактивен дольше определенного времени - помечаем как неактивного
                if (userSession.Status == UserOnlineStatus.Online && (nowTime - userSession.LastActivityTime) > inactivityTime)
                {
                    lock (_locker)
                    {
                        userSession.Status = UserOnlineStatus.NotActive;
                        profile.OnlineTime += onlineTime;
                        userSession.EnterTime = nowTime;
                    }
                    updatedProfiles.Add(profile);   // обновляем время онлайна в профиле
                    await _chatHub.Clients.All.AddUserToOnlineList(userSession); // рассылаем клиентам новые данные сессии юзера
                    
                    if (_isDevelopMode) _logger.LogInformation("Set 'Not Active' status for user '{0}' ", profile.Name);

                }
                // если юзер отмечен как оффлайн - удаляем из памяти
                else if (userSession.Status == UserOnlineStatus.Offline && (nowTime - userSession.LastActivityTime) > offlineInterval)
                {
                    lock (_locker)
                    {
                        if (onlineTime > 0) profile.OnlineTime += onlineTime;
                        OnlineUsers.Remove(userSession.UserId);
                    }
                    updatedProfiles.Add(profile);
                    // запись в истории об изменении имени
                    if(!userSession.OriginalName.Equals(profile.Name)) historyData.Add(new UserHistory
                    {
                        Date = nowTime,
                        Type = ActivityTypes.ChangeName,
                        UserID = userSession.UserId,
                        Description = $"{userSession.OriginalName}>{profile.Name}"
                    });

                    historyData.Add(new UserHistory
                    {
                        Date = nowTime,
                        Type = ActivityTypes.Visit,
                        UserID = userSession.UserId,
                        Description = onlineTime.ToString()
                    });
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
                    if (_isDevelopMode) _logger.LogInformation("User '{0}' romoved from online users.", profile.Name);
                }

                // обновляем кеш
                User user = null;
                if (_memoryCache.TryGetValue<User>(profile.UserId, out user))
                {
                    user.Profile = profile;
                    _memoryCache.Set<User>(user.Id, user, _cacheOptions);
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