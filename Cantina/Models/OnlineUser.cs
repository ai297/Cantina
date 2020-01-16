using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Класс хранит данные о юзере, который в данный момент находится в онлайне.
    /// </summary>
    public class OnlineUser
    {
        /// <summary>
        /// Сам юзер.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Id подключения клиента юзера к хабу. Один юзер может иметь только одно подключение.
        /// При попытке зайти в чат из новой вкладки или с другого устройства - старое подключение будет закрыто.
        /// </summary>
        public string HubConnectionId { get; set; }

        public UserOnlineStatus Status { get; set; }
    }
}
