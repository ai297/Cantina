using System;
using System.Linq;
using System.Collections.Generic;
using Cantina.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Cantina.Controllers;

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
            MessageService messageService, IHubContext<MainHub, IChatClient> hub, IWebHostEnvironment environment)
        {
            _services = serviceProvider;
            _logger = logger;
            _chatHub = hub;
            _messageService = messageService;
            _isDevelopMode = environment.IsDevelopment();
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public async Task AddUser(int userId, string connectionId)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                using var scope = _services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var userProfile = userService.GetUserProfile(userId);
                lock(_locker)
                {
                    OnlineUsers.Add(userId, new OnlineSession(connectionId, userProfile));
                }
                
                if(_isDevelopMode) _logger.LogInformation("User '{0}' connected to chat", userProfile.Name);

                // рассылаем сообщение о входе
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
            // если юзер в списке уже есть - меняем статус на онлайн и добавляем новое соединение
            else
            {
                lock (_locker)
                {
                    OnlineUsers[userId].AddConnection(connectionId);
                    OnlineUsers[userId].Status = UserOnlineStatus.Online;
                }
                
                if(_isDevelopMode) _logger.LogInformation("User '{0}' is online", OnlineUsers[userId].Name);
            }
        }

        /// <summary>
        /// Отключение клиента / юзера
        /// </summary>
        public void RemoveConnection(int userId, string connectionId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                lock(_locker)
                {
                    OnlineUsers[userId].RemoveConnection(connectionId);
                    if (OnlineUsers[userId].Connections == 0)
                    {
                        OnlineUsers[userId].Status = UserOnlineStatus.Offline;
                        
                        if(_isDevelopMode) _logger.LogInformation("User '{0}' is offline", OnlineUsers[userId].Name);
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет юзера из списка онлайна в случае, если у него не более одного активного соединения
        /// сохраняем в профиле время онлайна и записываем визит в историю дейстивий
        /// </summary>
        public async Task RemoveUser(int userId)
        {
            if (OnlineUsers.ContainsKey(userId) && OnlineUsers[userId].Connections <= 1)
            {

                var onlineTime = Convert.ToInt32((DateTime.UtcNow - OnlineUsers[userId].EnterTime).TotalMinutes);
                // если больше 3х минут - сохраняем в профиле и в истории визитов
                if (onlineTime > 3)
                {
                    using var scope = _services.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                    var historyService = scope.ServiceProvider.GetRequiredService<HistoryService>();
                    var profile = OnlineUsers[userId].GetProfile();
                    profile.OnlineTime += onlineTime;
                    await userService.UpdateUserProfileAsync(profile);
                    await historyService.NewActivityAsync(userId, ActivityTypes.Visit, onlineTime.ToString());
                }

                // рассылаем уведомление о выходе из чата
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
                await _chatHub.Clients.All.RemoveUserFromOnlineList(userId);

                if(_isDevelopMode) _logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, onlineTime);

                lock (_locker)
                {
                    OnlineUsers.Remove(userId);
                }

            }
            
        }

        /// <summary>
        /// Метод удаляет из списка всех юзеров со статусом offline
        /// </summary>
        public async Task CheckUsersStatus()
        {
            using var scope = _services.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var historyService = scope.ServiceProvider.GetRequiredService<HistoryService>();

            foreach (var keyValues in OnlineUsers)
            {
                var userSession = keyValues.Value;
                if(userSession.Status == UserOnlineStatus.Offline)
                {
                    var profile = userSession.GetProfile();
                    var onlineTime = Convert.ToInt32((userSession.LastActivityTime - userSession.EnterTime).TotalMinutes);
                    profile.OnlineTime += onlineTime;
                    await userService.UpdateUserProfileAsync(profile);
                    await historyService.NewActivityAsync(userSession.UserId, ActivityTypes.Visit, onlineTime.ToString());

                    lock (_locker)
                    {
                        OnlineUsers.Remove(userSession.UserId);
                    }

                    var message = new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = profile.Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System,
                        Text = "<author /> куда-то пропадает..."
                    };

                    _messageService.AddMessage(message);

                    await _chatHub.Clients.All.ReceiveMessage(message);
                    await _chatHub.Clients.All.RemoveUserFromOnlineList(profile.UserId);

                    if(_isDevelopMode) _logger.LogInformation("User '{0}' romoved from online users.", profile.Name, onlineTime);
                }
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


        public bool UpdateUserProfileInSession(UserProfile profile)
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