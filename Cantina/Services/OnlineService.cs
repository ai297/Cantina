using System;
using System.Linq;
using System.Collections.Generic;
using Cantina.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Cantina.Controllers;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис хранит список юзеров онлайн.
    /// </summary>
    public class OnlineService
    {
        // сервисы
        private readonly IServiceProvider Services;
        private readonly ILogger<OnlineService> Logger;
        private readonly IHubContext<MainHub, IChatClient> ChatHub;

        // список юзеров в онлайне
        private Dictionary<int, OnlineSession> OnlineUsers;
        // блокировщик для изменений списка юзеров
        private static object _locker = new object();

        public OnlineService(IServiceProvider serviceProvider, ILogger<OnlineService> logger, IHubContext<MainHub, IChatClient> hub)
        {
            OnlineUsers = new Dictionary<int, OnlineSession>();          // список юзеров онлайн
            Services = serviceProvider;
            Logger = logger;
            ChatHub = hub;
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public async Task AddUser(int userId, string connectionId)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                using var scope = Services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var userProfile = userService.GetUserProfile(userId);
                lock(_locker)
                {
                    OnlineUsers.Add(userId, new OnlineSession(connectionId, userProfile));
                }

                // рассылаем сообщение о входе
                await ChatHub.Clients.All.ReceiveMessage( new ChatMessage
                {
                    AuthorId = 0,
                    AuthorName = userProfile.Name,
                    DateTime = DateTime.UtcNow,
                    Type = MessageTypes.System.ToString(),
                    Text = "В Кантину заходит <author />."
                });
                await ChatHub.Clients.AllExcept(connectionId).AddUserToOnlineList(OnlineUsers[userId]);

                Logger.LogInformation("User '{0}' connected to chat", userProfile.Name);
            }
            // если юзер в списке уже есть - меняем статус на онлайн и добавляем новое соединение
            else
            {
                lock (_locker)
                {
                    OnlineUsers[userId].AddConnection(connectionId);
                    OnlineUsers[userId].Status = UserOnlineStatus.Online;
                }
                Logger.LogInformation("User '{0}' is online", OnlineUsers[userId].Name);
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
                        Logger.LogInformation("User '{0}' is offline", OnlineUsers[userId].Name);
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
                    using var scope = Services.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                    var historyService = scope.ServiceProvider.GetRequiredService<UsersHistoryService>();
                    var profile = OnlineUsers[userId].GetProfile();
                    profile.OnlineTime += onlineTime;
                    await userService.UpdateUserProfileAsync(profile);
                    await historyService.NewActivityAsync(userId, ActivityTypes.Visit, onlineTime.ToString());
                }

                // рассылаем уведомление о выходе из чата
                await ChatHub.Clients.All.ReceiveMessage(new ChatMessage
                {
                    AuthorId = 0,
                    AuthorName = OnlineUsers[userId].Name,
                    DateTime = DateTime.UtcNow,
                    Type = MessageTypes.System.ToString(),
                    Text = "<author /> покидает Кантину.",
                });
                await ChatHub.Clients.All.RemoveUserFromOnlineList(userId);

                Logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, onlineTime);

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
            using var scope = Services.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var historyService = scope.ServiceProvider.GetRequiredService<UsersHistoryService>();

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

                    await ChatHub.Clients.All.ReceiveMessage(new ChatMessage
                    {
                        AuthorId = 0,
                        AuthorName = profile.Name,
                        DateTime = DateTime.UtcNow,
                        Type = MessageTypes.System.ToString(),
                        Text = "<0> куда-то пропадает..."
                    });
                    await ChatHub.Clients.All.RemoveUserFromOnlineList(profile.UserId);

                    Logger.LogInformation("User '{0}' romoved from online users.", profile.Name, onlineTime);
                }
            }
        }


        /// <summary>
        /// Сессия конкретного юзера вонлайне или null, если юзера нет.
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
                   where keyValue.Value.Status != UserOnlineStatus.Hidden   // Исключаем невидимсых.
                   && keyValue.Value.Status != UserOnlineStatus.Offline     // И тех, кто уже отключился
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