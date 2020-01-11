using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
        DataContext database;
        IConfiguration configuration;
        IHashService hashService;

        public AuthController(DataContext dataContext, IConfiguration configuration, IHashService hashService)
        {
            this.database = dataContext;            // подключаем контекст базы данных
            this.configuration = configuration;     // подключаем конфигурацию
            this.hashService = hashService;         // сервис хэширования
        }

        /// <summary>
        /// Метод доступен по адресу Auth/Login. Ищет юзера в бд по email и поверяет пароль. В случае успеха возвращает токен авторизации.
        /// </summary>
        /// <param name="request">Email и Password в теле запроса.</param>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult<TokenResponse> Login([FromBody] LoginRequest request)
        {
            // ищем юзера по email
            var user = database.Users.SingleOrDefault<User>(u => u.Email == request.Email.ToLower());
            // если не нашли
            if (user == null) return NotFound("User not found");

            var userAuth = user.GetPasswordHash();
            // если пароль верный и аккаунт подтверждён / активен - генерируем и возвращаем токен авторизации
            // TODO: сделать кэширование профиля авторизованного юзера, что бы не лазить за ним в базу каждый раз
            if (userAuth.Item1 == hashService.GetHash(request.Password, userAuth.Item2).Item1 && user.Confirmed && user.Active)
            {
                return Ok(GetTokenResponse(user));
            }
            else return Unauthorized("Invalid email or password.");
        }

        /// <summary>
        /// Метод Auth/Refresh проверяет рефреш-токен и в случае его валидности - обновляет токен авторизации и сам рефреш-токен.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthOptions.ClaimUA)]
        public ActionResult<TokenResponse> Refresh()
        {
            // получаем 
            var ClaimId = HttpContext.User.FindFirstValue(AuthOptions.ClaimID);
            var ClaimUA = HttpContext.User.FindFirstValue(AuthOptions.ClaimUA);
            var ClaimName = HttpContext.User.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);

            var hashedUserAgent = hashService.SimpleHash(HttpContext.Request.Headers["User-Agent"]);

            // если не установлен claim с Id пользователя или маркер рефреш-токена (юзер агент)
            // или значение клэйма юзер-агента не равно текущему хэшу User-Agent
            // то возвращаем код 401
            if (String.IsNullOrEmpty(ClaimId) ||
                String.IsNullOrEmpty(ClaimUA) ||
                ClaimUA != hashedUserAgent) return Unauthorized();

            // ищем юзера по Id
            var user = database.Users.SingleOrDefault<User>(u => u.Id == Convert.ToInt32(ClaimId));
            // если юзер не найден или аккаунт не подтверждён / не активен
            if (user == null || !user.Active || !user.Confirmed) return NotFound("Invalid uiser");
            // если юзер изменил email то рефреш-токен не действителен
            if (user.Email != ClaimName) return Unauthorized();
            
            // если всё впорядке - обновляем и возвращаем оба токена
            return Ok(GetTokenResponse(user));
        }

        /// <summary>
        /// Метод генерирует токены авторизации.
        /// </summary>
        private TokenResponse GetTokenResponse(User user)
        {
            // генерируем access-токен
            var expires = DateTime.UtcNow.AddMinutes(AuthOptions.TokenLifetime);    // срок действия токена

            var token = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                claims: new Claim[]
                {
                    new Claim(AuthOptions.ClaimID, user.Id.ToString()),                                 // токен хранит Id юзера
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),                         // email в качестве username
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())                // роль (админ / юзер)
                },
                expires: expires,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration), AuthOptions.SecurityAlgorithm)
            );
            var accessJWT = new JwtSecurityTokenHandler().WriteToken(token);

            // генерируем refresh-токен
            var refreshExpires = DateTime.UtcNow.AddHours(AuthOptions.RefreshLifetime);
            var ClaimUAValue = hashService.SimpleHash(HttpContext.Request.Headers["User-Agent"]);
            token = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                claims: new Claim[]
                {
                    new Claim(AuthOptions.ClaimID, user.Id.ToString()),                 // записываем в рефреш-токен Id юзера
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),         // email в качестве username
                    new Claim(AuthOptions.ClaimUA, ClaimUAValue)                        // хэш заголовка юзер-агента (данный клэйм отличает обычный токен от рефреш-токена
                },
                expires: refreshExpires,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(configuration), AuthOptions.SecurityAlgorithm)
            );
            var refreshJWT = new JwtSecurityTokenHandler().WriteToken(token);

            // Формируем ответ
            return new TokenResponse
            {
                UserId = user.Id,
                AccessToken = accessJWT,
                AccessExpires = expires,
                RefreshToken = refreshJWT,
                RefreshExpires = refreshExpires
            };
        }
    }
}