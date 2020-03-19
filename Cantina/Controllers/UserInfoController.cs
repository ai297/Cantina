using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает информацию о юзере
    /// </summary>
    public class UserInfoController : ApiBaseController
    {
        UserService userService;

        public UserInfoController(UserService userService)
        {
            this.userService = userService;
        }
        
        /// <summary>
        /// На запрос без параметров возвращается полная информация о текущем юзере
        /// </summary>
        [HttpGet]
        public ActionResult GetUserInfo()
        {
            var userId = HttpContext.User.FindFirstValue(AuthOptions.Claims.ID);
            if (String.IsNullOrEmpty(userId)) return Unauthorized();

            var user = userService.GetUser(Convert.ToInt32(userId));
            if (user != null) return Ok(user);
            else return BadRequest();
        }

        /// <summary>
        /// На запрос с указанием конкретного Id юзера возвращается общая информация о юзере
        /// </summary>
        [HttpGet("{userId}")]
        public ActionResult GetUserInfo(int userId)
        {
            var user = userService.GetUser(userId);
            if (user == null) return BadRequest();
            else return Ok(new
            {
                Id = user.Id,
                Name = user.Name,
                Gender = user.Gender,
                Location = user.Location,
                Birthday = user.Birthday,
                OnlineTime = user.OnlineTime,
                BannedTo = user.EndBlockDate
            });
        }
    }
}
