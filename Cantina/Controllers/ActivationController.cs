using Cantina.Models.Requests;
using Cantina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер для активации аккаунтов
    /// </summary>
    public class ActivationController : ApiBaseController
    {
        private readonly UserService _userService;
        private readonly TokenGenerator _tokenGenerator;

        public ActivationController(UserService userService, TokenGenerator tokenGenerator)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
        }

        //GET activation/email/user@mail.net
        /// <summary>
        /// Запрос возвращает код активации для заданного e-mail'a. Доступен должен быть только админами для ручной активации юзеров.
        /// </summary>
        [Authorize(Policy = ChatConstants.AuthPolicy.RequireAdminRole), HttpGet("{email}")]
        public ActionResult GetActivationCode(string email, [FromServices] IOptions<AuthOptions> options)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Кто?");
            return Ok($"Код активации для {email} (действует {options.Value.ActivationTokenLifetime} дней с текущего момента):\n {_tokenGenerator.GetActivationToken(email)}");
        }

        /// <summary>
        /// Активация аккаунта
        /// </summary>
        [Authorize(Policy = ChatConstants.AuthPolicy.ConfirmAccaunt), HttpPut]
        public async Task<ActionResult> ActivateAccaunt()
        {
            var claimEmail = HttpContext.User.FindFirstValue(ChatConstants.Claims.Email);
            var user = _userService.GetUser(claimEmail);
            if (user == null) return NotFound("Аккаунт не найден.");
            if (user.Confirmed == true) return Ok("Аккаунт уже был активирован.");
            if (await _userService.Activate(user)) return Ok("Аккаунт успешно активирован. Добро пожаловать в Кантину!");
            else return BadRequest("Активация не удалась."); ;
        }

    }
}