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
            return Ok(OnlineService.GetOnlineUsers());
        }
    }
}
