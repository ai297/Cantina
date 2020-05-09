using Cantina.Models;
using Cantina.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает информацию о юзере
    /// </summary>
    public class UserInfoController : ApiBaseController
    {
        private readonly OnlineUsersService _onlineUsers;

        public UserInfoController(OnlineUsersService onlineService)
        {
            _onlineUsers = onlineService;
        }

        /// <summary>
        /// На запрос без параметров возвращается полная информация о текущем юзере
        /// </summary>
        [HttpGet]
        public ActionResult GetUserInfo()
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ChatConstants.Claims.ID));
            return GetUserInfo(userId);
        }

        /// <summary>
        /// На запрос с указанием конкретного Id юзера возвращается общая информация о юзере
        /// </summary>
        [HttpGet("{userId}")]
        public ActionResult GetUserInfo(int userId)
        {
            var userSession = _onlineUsers.GetSessionInfo(userId);

            if (userSession == null) return BadRequest("Юзер не в сети");
            else
            {
                var profile = userSession.GetProfile();
                return Ok(profile);
            }
        }

        /// <summary>
        /// Обновление профиля юзера в списке онлайна. Обновить профиль оффлайновому юзеру этим методом нельзя
        /// </summary>
        [HttpPatch]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UserProfile request,
            [FromServices] UserService userService,
            [FromServices] IHubContext<MainHub, IChatClient> mainHub)
        {
            // 1. Проверяем данные запроса
            if (!TryValidateModel(request)) return BadRequest("Некорректный запрос.");
            
            // 2. Получаем ID и роль юзера из токена авторизации
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue(ChatConstants.Claims.ID));
            var userRole = HttpContext.User.FindFirstValue(ChatConstants.Claims.Role);
            
            // 3. Проверяем подмену id юзера - чужой профиль может менять только админ
            if (userId != request.UserId && !userRole.Equals(UserRoles.Developer.ToString())) return Forbid("Недостаточно прав доступа.");

            // 4. Ищем юзера в списке онлайна, если его там нет - ошибка.
            var userSession = _onlineUsers.GetSessionInfo(userId);
            if(userSession == null || userSession.Connections == 0) return BadRequest("Профиль не найден.");

            // 5. Проверяем есть ли изменения в профиле. Если нет изменений - больше ничего не делаем.
            var profile = userSession.GetProfile();
            if (request == profile) return Ok();

            // 6. Если изменено имя - проверяем новое на доступность
            var isNameChange = !profile.Name.Equals(request.Name);
            if (isNameChange && userService.CheckNameForForbidden(request.Name))
                return BadRequest("Имя уже занято либо запрещено.");

            // 7. Фиксим изменение времени онлайна
            request.OnlineTime = profile.OnlineTime;

            // 8. Сохраняем изменения профиля в памяти (только в списке онлайна, не в базе - в бае обновятся данные при выходе из чата)
            if (_onlineUsers.UpdateUserProfile(request))
            {
                if (isNameChange) await userService.UpdateForbiddenName(userId, request.Name); // обновить имя в таблице запрещенных имен
                if (userSession.Status != UserOnlineStatus.Hidden && userSession.Connections > 0) await mainHub.Clients.All.AddUserToOnlineList(userSession);
                userSession.LastActivityTime = DateTime.UtcNow;
                return Ok("Профиль успешно обновлён.");
            }
            else return BadRequest("Не удалось обновить профиль");
        }
    }
}