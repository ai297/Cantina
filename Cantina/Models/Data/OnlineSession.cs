using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Онлайн сессия юзера.
    /// </summary>
    public class OnlineSession
    {
        public User User { get; private set; }
        public DateTime EnterTime { get; private set; }
        public UserOnlineStatus Status { get; set; } = UserOnlineStatus.Online;
        public List<string> ConnectionIDs { get; set; }

        public OnlineSession(User user, string connectionId)
        {
            User = user;
            EnterTime = DateTime.UtcNow;
            ConnectionIDs = new List<string> { connectionId };
        }

    }
}
