using System;
using System.Collections.Generic;

namespace Cantina.Models
{
    /// <summary>
    /// "Публичная" информация о пользователе, доступная всем
    /// </summary>
    public class UserInfoResponse
    {
        public string Name { get; set; }
        public UserOnlineStatus OnlineStatus { get; set; } = UserOnlineStatus.Offline;
        public DateTime? EnterTime { get; set; } = null;
        public UserProfile Profile { get; set; }
    }
}
