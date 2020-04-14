using Cantina.Services;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(OnlineService.GetOnlineUsers());
        }
    }
}
