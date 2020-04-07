using System;
using System.Linq;
using System.Collections.Generic;
using Cantina.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис хранит список юзеров онлайн.
    /// </summary>
    public class OnlineService
    {
        IServiceProvider Services { get; }
        ILogger<OnlineService> Logger { get; }

        private Dictionary<int, OnlineSession> OnlineUsers;


        public OnlineService(IServiceProvider serviceProvider, ILogger<OnlineService> logger)
        {
            OnlineUsers = new Dictionary<int, OnlineSession>();          // список юзеров онлайн
            Services = serviceProvider;
            Logger = logger;
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public bool AddUser(int userId, string connectionId)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                using var scope = Services.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var userProfile = userService.GetUserProfile(userId);

                OnlineUsers.Add(userId, new OnlineSession(connectionId, userProfile));

                Logger.LogInformation("User '{0}' connected to chat", userProfile.Name);

                // TODO: действие, что бы отправить сообщение о входе в чат
                return true;
            }
            else OnlineUsers[userId].AddConnection(connectionId);
            // в этом случае сообщение о входе в чат не отправляется
            return false;
        }

        /// <summary>
        /// Отключение клиента / юзера
        /// </summary>
        public async Task<bool> RemoveUser(int userId, string connectionId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                OnlineUsers[userId].RemoveConnection(connectionId);
                // если у юзера не осталось активных подключений - отключаем его
                if (OnlineUsers[userId].Connections == 0)
                {
                    // время юзера в онлайне
                    var onlineTime = Convert.ToInt32((DateTime.UtcNow - OnlineUsers[userId].EnterTime).TotalMinutes);
                    // если больше 3х минут - сохраняем в профиле и в истории визитов
                    if(onlineTime > 3)
                    {
                        using var scope = Services.CreateScope();
                        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                        var historyService = scope.ServiceProvider.GetRequiredService<UsersHistoryService>();
                        var profile = OnlineUsers[userId].GetProfile();
                        profile.OnlineTime += onlineTime;
                        await userService.UpdateUserProfileAsync(profile);
                        await historyService.NewActivityAsync(userId, ActivityTypes.Visit, onlineTime.ToString());
                    }
                    // лог об отключении юзера
                    Logger.LogInformation("User '{0}' disconnected after {1} min.", OnlineUsers[userId].Name, onlineTime);
                    // удаляем юзера из списка онлайн
                    OnlineUsers.Remove(userId);
                    // TODO: здесь вызывается действие для отправки уведомления о выходе
                    return true;
                }
                // else - в этом случае (ещё остались активные соединения) - сообщения о выходе отправлять не нужно
            }
            // Если юзера нет в списке онлайна - ничего делать не надо
            return false;
        }

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
                OnlineUsers[profile.UserId].Profile = profile;
                // TODO: Здесь уведомление об обновлении профиля юзера?
                return true;
            }
            return false;
        }

    }
}