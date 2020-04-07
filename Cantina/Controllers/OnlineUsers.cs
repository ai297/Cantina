using System;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает список юзеров онлайн
    /// </summary>
    public class OnlineUsers : ApiBaseController
    {
        OnlineService OnlineService;

        public OnlineUsers(OnlineService onlineService)
        {
            OnlineService = onlineService;
        }

        [HttpGet]
        public ActionResult GetOnlineUsers()
        {
            var OnlineUsers = OnlineService.GetOnlineUsers();
            return Ok(OnlineUsers);
        }
    }
}
