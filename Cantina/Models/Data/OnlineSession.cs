using System;
using System.Collections.Generic;

namespace Cantina.Models
{
    /// <summary>
    /// Онлайн сессия юзера.
    /// </summary>
    public class OnlineSession
    {
        #region Данные юзера
        /// <summary>
        /// Профиль юзера
        /// </summary>
        public UserProfile Profile { private get; set; }
        /// <summary>
        /// Имя юзера
        /// </summary>
        public string Name { get => Profile.Name; }
        /// <summary>
        /// ID юзера
        /// </summary>
        public int UserId { get => Profile.UserId; }
        /// <summary>
        /// CSS стиль отображения ника
        /// </summary>
        public string NameStyle
        {
            get
            {
                if (Profile.Settings != null && Profile.Settings.NameStyle != null) return Profile.Settings.NameStyle.ToString();
                else return "";
            }
        }
        /// <summary>
        /// CSS стиль отображения сообщения
        /// </summary>
        public string MessageStyle
        {
            get
            {
                if (Profile.Settings != null && Profile.Settings.MessageStyle != null) return Profile.Settings.MessageStyle.ToString();
                else return "";
            }
        }

        public UserProfile GetProfile()
        {
            return Profile;
        }
        #endregion

        #region Данные сессии
        /// <summary>
        /// Время входа в чат
        /// </summary>
        public DateTime EnterTime { get; set; }
        /// <summary>
        /// Статус сессии - в сети, не в сети, отошёл и т.д.
        /// </summary>
        public UserOnlineStatus Status { get; set; }
        /// <summary>
        /// Время последней активности
        /// </summary>
        public DateTime LastActivityTime { get; set; }
        /// <summary>
        /// Список id активных клиентов юзера
        /// </summary>
        private HashSet<string> ConnectionIDs;
        /// <summary>
        /// Количество активных подключений юзера
        /// </summary>
        public int Connections { get => ConnectionIDs.Count; }
        /// <summary>
        /// Добавить новое подключение
        /// </summary>
        public bool AddConnection(string connectionId)
        {
            return ConnectionIDs.Add(connectionId);
        }
        /// <summary>
        /// Удалить конкретное подключение
        /// </summary>
        public bool RemoveConnection(string connectionId)
        {
            return ConnectionIDs.Remove(connectionId);
        }
        #endregion

        /// <summary>
        /// Исходное имя юзера при логине
        /// </summary>
        public string OriginalName { get; }

        public OnlineSession(string connectionId, UserProfile userProfile, UserOnlineStatus onlineStatus = UserOnlineStatus.Online)
        {
            Profile = userProfile;
            EnterTime = DateTime.UtcNow;
            ConnectionIDs = new HashSet<string> { connectionId };
            LastActivityTime = DateTime.UtcNow;
            Status = onlineStatus;
            OriginalName = userProfile.Name;
        }
    }
}
