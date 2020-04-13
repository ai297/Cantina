using System;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models;
using System.Collections.Generic;

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
