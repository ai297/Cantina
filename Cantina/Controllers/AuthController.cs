using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер авторизации. Возвращает JWT (токен авторизации) в обмен на валидные login/password или действующий RefreshToken
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        UserService userService;
        TokenGenerator tokenGenerator;
        IHashService hashService;

        public AuthController(UserService userService, TokenGenerator tokenGenerator, IHashService hashService)
        {
            this.userService = userService;
            this.tokenGenerator = tokenGenerator;
            this.hashService = hashService;
        }

        /// <summary>
        /// Метод доступен по адресу Auth/Login. Ищет юзера в бд по email и поверяет пароль. В случае успеха возвращает токен авторизации.
        /// </summary>
        /// <param name="request">Email и Password в теле запроса.</param>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            // Ищем юзера по email и проверяем пароль.
            var user = await userService.GetUser(request.Email);
            // Если не нашли - не авторизован.
            if (user == null) return Unauthorized(new ErrorResponse { Message = "Неверный логин" });
            // Сравниваем хэши паролей.
            var userHashedPassword = user.GetHashedPassword();
            var hashedPassword = hashService.GetHash(request.Password, userHashedPassword.Salt);
            if(!userHashedPassword.Hash.Equals(hashedPassword.Hash)) return Unauthorized(new ErrorResponse { Message = "Неверный пароль" });
            // Возвращаем токены.
            var userAgent = HttpContext.Request.Headers["User-Agent"];
            return Ok(tokenGenerator.GetTokenResponse(user, userAgent));
        }

        /// <summary>
        /// Метод Auth/Refresh проверяет рефреш-токен и в случае его валидности - обновляет токен авторизации и сам рефреш-токен.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AuthOptions.ClaimUA)]
        public async Task<ActionResult<TokenResponse>> Refresh()
        {
            // получаем информацию о юзере из клэймов, сохранённых в токене
            var ClaimId = HttpContext.User.FindFirstValue(AuthOptions.ClaimID);
            var ClaimUA = HttpContext.User.FindFirstValue(AuthOptions.ClaimUA);
            var ClaimName = HttpContext.User.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);

            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var hashedUserAgent = hashService.SimpleHash(userAgent);

            // если не установлен claim с Id пользователя или маркер рефреш-токена (юзер агент)
            // или значение клэйма юзер-агента не равно текущему хэшу User-Agent (другой браузер или другое устройство)
            // то возвращаем код 401
            if (String.IsNullOrEmpty(ClaimId) ||
                String.IsNullOrEmpty(ClaimUA) ||
                ClaimUA != hashedUserAgent) return Unauthorized();

            // ищем юзера по Id
            var user = await userService.GetUser(Convert.ToInt32(ClaimId));
            // если юзер не найден или аккаунт не подтверждён / не активен
            if (user == null || !user.Active || !user.Confirmed) return Unauthorized();
            // если юзер изменил email то рефреш-токен не действителен
            if (user.Email != ClaimName) return Unauthorized();

            // если всё впорядке - обновляем и возвращаем оба токена
            return Ok(tokenGenerator.GetTokenResponse(user, userAgent));
        }
    }
}