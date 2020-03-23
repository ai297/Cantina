using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models.Response;

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
            else return Ok(new PublicUserInfo(user));
        }
    }
}
