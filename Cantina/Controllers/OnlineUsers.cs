using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models;
using Cantina.Models.Response;

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
            var OnlineUsers = OnlineService.GetOnlineUsers().ConvertAll(
                new Converter<OnlineSession, OnlineSessionInfo>(OnlineSessionInfo.OnlineSessionOut)
                );
            
            return Ok(OnlineUsers);
        }
    }
}
