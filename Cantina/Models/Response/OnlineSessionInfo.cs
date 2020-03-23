using System;

namespace Cantina.Models.Response
{
    /// <summary>
    /// Информация о текущей сессии юзера
    /// </summary>
    public class OnlineSessionInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime EnterTime { get; set; }
        public UserOnlineStatus Status { get; set; }
        public int Connections { get; set; }

        public static OnlineSessionInfo OnlineSessionOut (OnlineSession onlineSession)
        {
            return new OnlineSessionInfo
            {
                Id = onlineSession.User.Id,
                Name = onlineSession.User.Name,
                EnterTime = onlineSession.EnterTime,
                Status = onlineSession.Status,
                Connections = onlineSession.ConnectionIDs.Count
            };
        }
    }
}
