using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис обрабатывает подключения и отключения юзеров.
    /// </summary>
    public class ConnectionService
    {
        private UsersHistoryService historyService;
        private UserService userService;
        private UsersOnlineService onlineService;

        public ConnectionService(UserService userService, UsersHistoryService historyService, UsersOnlineService onlineService)
        {
            this.historyService = historyService;
            this.userService = userService;
            this.onlineService = onlineService;
        }

        #region подключение - отключение
        /// <summary>
        /// Метод добавляет юзера в список онлайна, если его там ещё нет. Если есть - то обновляет Id подключенного клиента.
        /// </summary>
        /// <param name="connectionId">Id подключения к хабу</param>
        public async void UserConnect(int userId, string connectionId, UserOnlineStatus status = UserOnlineStatus.Online)
        {
            var user = onlineService.GetUser(userId);
            if (user != null)
            {
                // Если юзер онлайн - обновляем id соединения
                if (!user.HubConnectionId.Equals(connectionId))
                {
                    onlineService.UpdateConnectionId(userId, connectionId);
                }
                user.OnlineStatus = status;
            }
            else
            {
                // если юзера ещё нет в списке онлайн - добавляем.
                user = await userService.GetUser(userId);
                user.HubConnectionId = connectionId;
                user.LastEnterTime = DateTime.Now;
                user.OnlineStatus = status;
                onlineService.AddUser(user);
            }
        }

        /// <summary>
        /// Метод удаляет юзера из списка онлайн.
        /// </summary>
        public void UserDisconnect(int uid, string connectionId)
        {
            var user = onlineService.GetUser(uid);
            if (user == null) return;   // Если юзера нет в списке онлайн - ничего не делаем.
            // Если id отключившегося клиента и id подключения текущего юзера не совпадают - ничего не делаем.
            if (user.HubConnectionId != connectionId) return;
            var onlineTime = Convert.ToInt32((DateTime.Now - user.LastEnterTime).TotalMinutes);     // время онлайна
            historyService.NewActivityAsync(user, ActivityTypes.Visit, onlineTime.ToString());      // сохраняем визит в историю действий
            // Обновляем профиль.
            user.Profile.OnlineTime += onlineTime;
            user.OnlineStatus = UserOnlineStatus.Offline;
            userService.UpdateUserAsync(user);
            // Удаляем юзера из списка онлайн.
            onlineService.RemoveUser(uid);
        }

        #endregion

    }
}
