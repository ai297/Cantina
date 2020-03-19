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
        Dictionary<int, UserInOnline> OnlineUsers;

        public OnlineService()
        {
            OnlineUsers = new Dictionary<int, UserInOnline>();          // список юзеров онлайн
        }

        /// <summary>
        /// Подключение нового клиента
        /// </summary>
        public bool AddUser(int id, string connectionId, UserService userService)
        {
            // Если юзера нет в списке онлайн - добавляем
            if (!OnlineUsers.ContainsKey(id))
            {
                var user = userService.GetUser(id);
                OnlineUsers.Add(user.Id, new UserInOnline(user, connectionId));
                return true;
            }
            if (!OnlineUsers[id].ConnectionIDs.Contains(connectionId)) OnlineUsers[id].ConnectionIDs.Add(connectionId);
            return false;
        }


        public UserInOnline GetUserIfOnline(int id)
        {
            if (OnlineUsers.ContainsKey(id)) return OnlineUsers[id];
            else return null;
        }
    }
}