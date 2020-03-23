using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models.Requests;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер авторизации. Возвращает JWT (токен авторизации) в обмен на валидные login/password
    /// </summary>
    public class AuthController : ApiBaseController
    {
        UserService userService;
        TokenGenerator tokenGenerator;
        HashService hashService;

        public AuthController(UserService userService, TokenGenerator tokenGenerator, HashService hashService)
        {
            this.userService = userService;
            this.tokenGenerator = tokenGenerator;
            this.hashService = hashService;
        }

        /// <summary>
        /// Метод ищет юзера в бд по email и поверяет пароль. В случае успеха возвращает токен авторизации.
        /// </summary>
        /// <param name="request">Email и Password в теле запроса.</param>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetToken([FromBody] LoginRequest request)
        {
            // проверяем запрос
            if (!TryValidateModel(request, nameof(request))) return BadRequest("Некорректные данные.");
            // Ищем юзера по email и проверяем пароль.
            var user = userService.GetUser(request.Email);
            // Если не нашли или не совпадает пароль - не авторизован.
            if (user == null || !user.PasswordEqual(hashService.Get256Hash(request.Password))) return Unauthorized("Неверный логин или пароль");
            // Если аккаунт заблокирован
            if (!user.Active) return Forbid();
            if (!user.Confirmed) return Ok(new { Success = false, Type = "activation" });
            // Генерируем и возвращаем токен
            var userAgent = hashService.Get128Hash(HttpContext.Request.Headers["User-Agent"]);
            return Ok( new { Success = true, Token = tokenGenerator.GetToken(user.Id, user.Email, user.Role, userAgent), UserName = user.Name });
        }

        /// <summary>
        /// Метод обновляет токен авторизации
        /// </summary>
        [HttpGet]
        public ActionResult GetNewToken()
        {
            // получаем информацию о юзере из клэймов, сохранённых в токене
            var ClaimId = HttpContext.User.FindFirstValue(AuthOptions.Claims.ID);
            var ClaimUA = HttpContext.User.FindFirstValue(AuthOptions.Claims.UserAgent);
            var ClaimEmail = HttpContext.User.FindFirstValue(AuthOptions.Claims.Email);
            var userAgent = hashService.Get128Hash(HttpContext.Request.Headers["User-Agent"]);

            // если не установлен claim с Id пользователя
            // или значение клэйма юзер-агента не равно текущему хэшу User-Agent (другой браузер или другое устройство)
            // то возвращаем код 401
            if (String.IsNullOrEmpty(ClaimId) ||
                String.IsNullOrEmpty(ClaimUA) ||
                !ClaimUA.Equals(userAgent)) return Unauthorized();

            // ищем юзера по Id
            var user = userService.GetUser(Convert.ToInt32(ClaimId));
            // если юзер не найден или аккаунт не подтверждён / не активен
            if (user == null || !user.Active || !user.Confirmed) return Unauthorized();
            // если юзер изменил email то рефреш-токен не действителен
            if (!user.Email.Equals(ClaimEmail)) return Unauthorized();

            // если всё впорядке - обновляем и возвращаем оба токена
            return Ok(new { Success = true, Token = tokenGenerator.GetToken(user.Id, user.Email, user.Role, userAgent), UserName = user.Name });
        }
    }
}