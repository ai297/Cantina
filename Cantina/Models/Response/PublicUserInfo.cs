using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models.Response
{
    /// <summary>
    /// Профиль юзера без лишней информации
    /// </summary>
    public class PublicUserInfo
    {
        public string Name { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime? Birthday { get; private set; }
        public string Location { get; private set; }
        public int OnlineTime { get; private set; }

        public PublicUserInfo(User user)
        {
            Name = user.Name;
            Gender = user.Gender;
            Birthday = user.Birthday;
            Location = user.Location;
            OnlineTime = user.OnlineTime;
        }
    }
}
