using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Models;
using Cantina.Services;
using System.Threading.Tasks;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер профиля посетителя.
    /// если указан ID - возвращает конкретного юзера
    /// </summary>
    public class UserInfoController : ApiBaseController
    {
        UsersOnlineService usersOnline;
        UserService userService;
        int currentUserId;

        public UserInfoController(UserService userService, UsersOnlineService usersOnline)
        {
            this.userService = userService;
            this.usersOnline = usersOnline;
            currentUserId = Convert.ToInt32(HttpContext.User.FindFirst(AuthOptions.ClaimID).Value);
        }

        /// <summary>
        /// Профиль текущего авторизованного юзера
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<User>> Get()
        {
            var user = await userService.GetUser(currentUserId);
            return Ok(user);
        }

        /// <summary>
        /// Публичный профиль юзера по Id
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var user = usersOnline.GetUser(id);     // профиль юзера, который онлайн
            //var user = await userService.GetUser(id);     // профиль любого юзера
            if (user == null) return NotFound();
            var userInfo = new UserInfoResponse
            {
                Name = user.Name,
                OnlineStatus = (user.OnlineStatus == UserOnlineStatus.Hidden) ? UserOnlineStatus.Offline : user.OnlineStatus,
                EnterTime = user.LastEnterTime,
                Profile = user.Profile
            };
            return Ok(userInfo);
        }

    }
}