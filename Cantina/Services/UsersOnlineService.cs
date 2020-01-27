using System;
using System.Linq;
using System.Collections.Generic;
using Cantina.Models;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Список юзеров онлайн. Должен подключаться как singleton
    /// </summary>
    public class UsersOnlineService
    {

        private Dictionary<int, User> onlineUsers;
        #region события
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
        public event Action<int> UserDisconnected;
        #endregion

        public UsersOnlineService()
        {
            this.onlineUsers = new Dictionary<int, User>();           // список юзеров онлайн
        }

        /// <summary>
        /// Добавить юзера в список, если его там нет, или обновить данные, если уже есть.
        /// </summary>
        public void AddUser(User user)
        {
            if (!onlineUsers.ContainsKey(user.Id))
            {
                onlineUsers.Add(user.Id, user);
                UserConnected?.Invoke(user);
            }
            else onlineUsers[user.Id] = user;
        }
        /// <summary>
        /// Удаляем юзера из списка онлайн.
        /// </summary>
        public void RemoveUser(int id)
        {
            if (onlineUsers.ContainsKey(id))
            {
                CloseConnection?.Invoke(onlineUsers[id].HubConnectionId);
                onlineUsers.Remove(id);
                UserDisconnected?.Invoke(id);
            }
        }
        /// <summary>
        /// Возвращает юзера из списка онлайн или null, если юзера нет в списке.
        /// </summary>
        public User GetUser(int id)
        {
            if (onlineUsers.TryGetValue(id, out var user)) return user;
            else return null;
        }
        /// <summary>
        /// Обновляет id соединения клиента юзера и уведомляет о необходимости закрыть старое соединение.
        /// </summary>
        public void UpdateConnectionId(int userId, string connectionId)
        {
            CloseConnection?.Invoke(onlineUsers[userId].HubConnectionId);
            onlineUsers[userId].HubConnectionId = connectionId;
        }
    }
}
