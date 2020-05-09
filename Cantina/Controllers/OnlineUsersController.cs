using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cantina.Services;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает список юзеров онлайн
    /// </summary>
    public class OnlineUsersController : ApiBaseController
    {
        OnlineUsersService OnlineService;

        public OnlineUsersController(OnlineUsersService onlineService)
        {
            OnlineService = onlineService;
        }

        [HttpGet]
        public ActionResult GetOnlineUsers()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ChatConstants.Claims.ID));
            var isAdmin = false;
            if (HttpContext.User.HasClaim(match => match.Type.Equals(ChatConstants.Claims.Role) && match.Value.Equals(UserRoles.Developer.ToString()))) isAdmin = true;
            return Ok(OnlineService.GetOnlineUsers(userId, isAdmin));
        }
    }
}
