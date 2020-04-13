using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Cantina.Models;
using Cantina.Services;
using Microsoft.AspNetCore.SignalR;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает информацию о юзере
    /// </summary>
    public class UserInfoController : ApiBaseController
    {
        UserService UserService;

        public UserInfoController(UserService userService)
        {
            this.UserService = userService;
        }
        
        /// <summary>
        /// На запрос без параметров возвращается полная информация о текущем юзере
        /// </summary>
        [HttpGet]
        public ActionResult GetUserInfo()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(AuthOptions.Claims.ID));
            return GetUserInfo(userId);
        }

        /// <summary>
        /// На запрос с указанием конкретного Id юзера возвращается общая информация о юзере
        /// </summary>
        [HttpGet("{userId}")]
        public ActionResult GetUserInfo(int userId)
        {
            // TODO: заменить выдачу на UserProfile
            var profile = UserService.GetUserProfile(userId);
            if (profile == null) return BadRequest();
            else return Ok(profile);
        }

        [HttpPatch]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UserProfile request, 
            [FromServices] OnlineUsersService onlineService,
            [FromServices] HistoryService historyService,
            [FromServices] ILogger<UserInfoController> logger,
            [FromServices] IHubContext<MainHub, IChatClient> mainHub)
        {
            if (!TryValidateModel(request)) return BadRequest("Некорректный запрос.");
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(AuthOptions.Claims.ID));
            var userRole = HttpContext.User.FindFirstValue(AuthOptions.Claims.Role);
            // проверяем подмену id юзера - чужой профиль может менять только админ
            if (userId != request.UserId && !userRole.Equals(UserRoles.Admin.ToString())) return Forbid("Недостаточно прав доступа.");
            var profile = UserService.GetUserProfile(userId);
            // если изменено имя - проверяем новое на доступность
            var isNameChange = !profile.Name.Equals(request.Name);
            if(isNameChange && UserService.CheckNameForForbidden(request.Name))
                return BadRequest("Имя уже занято либо запрещено.");
            // Фиксим изменение времени онлайна
            request.OnlineTime = profile.OnlineTime;

            var isUpdated = await UserService.UpdateUserProfileAsync(request);
            if (isUpdated)
            {
                // если изменено имя - сохраняем запись в лог и в историю
                if(isNameChange)
                {
                    await historyService.NewActivityAsync(userId, ActivityTypes.ChangeName, $"Изменил имя с '{profile.Name}' на '{request.Name}'");
                    logger.LogInformation($"User '{profile.Name}' change name to '{request.Name}'");
                }
                // если юзер онлайн - обновляем о нём информацию и рассылаем уведомление об этом клиентам
                if (onlineService.UpdateUserProfileInSession(request))
                {
                    var session = onlineService.GetSessionInfo(userId);
                    if (session.Status != UserOnlineStatus.Hidden && session.Status != UserOnlineStatus.Offline) await mainHub.Clients.All.AddUserToOnlineList(session);
                    session.LastActivityTime = DateTime.UtcNow;
                }
            }
            return Ok();
        }
    }
}