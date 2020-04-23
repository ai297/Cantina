using Cantina.Models.Requests;
using Cantina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер для активации аккаунтов
    /// </summary>
    public class ActivationController : ApiBaseController
    {
        UserService userService;
        HashService hashService;

        public ActivationController(UserService userService, HashService hashService)
        {
            this.userService = userService;
            this.hashService = hashService;
        }

        //GET activation/email/user@mail.net
        /// <summary>
        /// Запрос возвращает код активации для заданного e-mail'a. Доступен должен быть только админами для ручной активации юзеров.
        /// </summary>
        [Authorize(Policy = AuthOptions.AuthPolicy.RequireAdminRole), HttpGet("{email}")]
        public ActionResult GetActivationCode(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Кто?");
            return Ok($"Код активации для {email}: {getValidationCode(email)}");
        }

        /// <summary>
        /// Активация аккаунта
        /// </summary>
        [AllowAnonymous, HttpPut]
        public async Task<ActionResult> ActivateAccaunt(ActivationRequest request)
        {
            if (!TryValidateModel(request, nameof(request))) return BadRequest("Некорректный запрос.");

            var user = userService.GetUser(request.Email);

            if (user == null) return NotFound("Аккаунт не найден.");

            if (user.Confirmed == true) return Ok("Аккаунт уже был активирован.");

            if (request.ActivationCode.Equals(getValidationCode(user.Email)))
            {
                if (await userService.Activate(user)) return Ok("Аккаунт успешно активирован. Добро пожаловать в Кантину!");
                else return BadRequest("Что-то пошло не по плану..");
            }

            return BadRequest("Активация не удалась.");
        }

        private string getValidationCode(string value)
        {
            return hashService.Get128Hash(value).Substring(3, 10);
        }
    }
}