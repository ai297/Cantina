using System;
using System.Collections.Generic;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис для работы со списком юзеров онлайн. Должен подключаться как singleton
    /// </summary>
    public class UsersOnlineService
    {
        private Dictionary<int, OnlineUser> onlineUsers;

        /// <summary>
        /// Событие срабатывает когда определённое подключение клиента становится недействительным. Передаёт в делегат строку с Id подключения, которое требуется закрыть.
        /// </summary>
        public event Action<string> CloseConnection;
        /// <summary>
        /// Событие срабатывает при входе нового посетителя в чат
        /// </summary>
        public event Action<User> UserConnected;
        /// <summary>
        /// Событие срабатывает при удалении юзера из списка онлайн.
        /// </summary>
        public event Action<User> UserDisconnected;

        public UsersOnlineService()
        {
            this.onlineUsers = new Dictionary<int, OnlineUser>();           // список юзеров онлайн
        }

        /// <summary>
        /// Метод добавляет юзера в список онлайна, если его там ещё нет. Если есть - то обновляет Id подключенного клиента.
        /// </summary>
        /// <param name="user">Подключающийся юзер</param>
        /// <param name="connectionId">Id подключения к хабу</param>
        public void UserConnect(User user, string connectionId)
        {
            // если юзер уже есть в списке онлайн
            if (onlineUsers.ContainsKey(user.Id))
            {
                if (onlineUsers[user.Id].HubConnectionId.Equals(connectionId)) return;  // если id подключения клиента совпадает с сохранённым - ничего не делаем
                CloseConnection?.Invoke(onlineUsers[user.Id].HubConnectionId);          // если не совпадает - отправляем уведомление о необходимости закрть соединение
                onlineUsers[user.Id].HubConnectionId = connectionId;                    // и сохраняем новый id подключения к хабу
            }
            else
            {
                // если юзера ещё нет в списке онлайн - добавляем
                onlineUsers.Add(user.Id, new OnlineUser()
                {
                    User = user,
                    HubConnectionId = connectionId,
                    Status = UserOnlineStatus.Online
                });
                // и вызываем событие подключения нового юзера
                UserConnected?.Invoke(user);
            }
        }

        /// <summary>
        /// Метод удаляет юзера из списка онлайн.
        /// </summary>
        /// <param name="uid">Id юзера.</param>
        public void UserDisconnect(int uid)
        {
            if (!onlineUsers.ContainsKey(uid)) return;                      // Если юзера нет в списке онлайна - ничего не делаем.
            CloseConnection?.Invoke(onlineUsers[uid].HubConnectionId);      // Отправляем уведомление о закрытии соединения.
            UserDisconnected?.Invoke(onlineUsers[uid].User);                // Уведомляем об отключении юзера.
            onlineUsers.Remove(uid);                                        // Удаляем юзера из списка онлайн.
        }

        /// <summary>
        /// Метод возвращает список со всеми юзерами, кто находится онлайн.
        /// </summary>
        /// <returns>Список юзеров онлайн.</returns>
        public List<User> GetOnline()
        {
            var users = new List<User>();
            foreach(var onlineUser in onlineUsers.Values)
            {
                users.Add(onlineUser.User);
            }
            return users;
        }
    }
}
