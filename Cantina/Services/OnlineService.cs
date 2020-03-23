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
    public class OnlineService
    {
        private Dictionary<int, OnlineSession> OnlineUsers;

        public OnlineService()
        {
            OnlineUsers = new Dictionary<int, OnlineSession>();          // список юзеров онлайн
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public bool AddUser(int userId, string connectionId, UserService userService)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(userId))
            {
                var user = userService.GetUser(userId);
                OnlineUsers.Add(user.Id, new OnlineSession(user, connectionId));
                return true;
            }
            if (!OnlineUsers[userId].ConnectionIDs.Contains(connectionId)) OnlineUsers[userId].ConnectionIDs.Add(connectionId);
            return false;
        }

        /// <summary>
        /// Отключение клиента / юзера
        /// </summary>
        public bool RemoveUser(int userId, string connectionId)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                OnlineUsers[userId].ConnectionIDs.Remove(connectionId);
                var isUserDisconnected = OnlineUsers[userId].ConnectionIDs.Count == 0;
                if (isUserDisconnected) OnlineUsers.Remove(userId);
                return isUserDisconnected;
            }
            return false;
        }


        public OnlineSession GetUserIfOnline(int id)
        {
            if (OnlineUsers.ContainsKey(id)) return OnlineUsers[id];
            else return null;
        }

        public List<OnlineSession> GetOnlineUsers()
        {
            var result = from keyValue in OnlineUsers
                         where keyValue.Value.Status != UserOnlineStatus.Hidden
                         select keyValue.Value;
            return result.ToList();
        }

    }
}