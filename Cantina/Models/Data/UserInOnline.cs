using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    public class UserInOnline
    {
        public User User { get; private set; }
        public DateTime EnterTime { get; private set; }
        public UserOnlineStatus Status { get; set; } = UserOnlineStatus.Online;
        public List<string> ConnectionIDs { get; set; }

        public UserInOnline(User user, string connectionId)
        {
            User = user;
            EnterTime = DateTime.UtcNow;
            ConnectionIDs = new List<string> { connectionId };
        }

    }
}
