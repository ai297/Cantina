using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models.Requests;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер для активации аккаунтов
    /// </summary>
    public class ActivationController : ApiBaseController
    {
        UserService userService;
        HashService hashService;
        HistoryService historyService;

        public ActivationController(UserService userService, HashService hashService, HistoryService historyService)
        {
            this.userService = userService;
            this.hashService = hashService;
            this.historyService = historyService;
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

        [AllowAnonymous, HttpPut]
        public async Task<ActionResult> ActivateAccaunt(ActivationRequest request)
        {
            if (!TryValidateModel(request, nameof(request))) return BadRequest("Некорректный запрос.");

            var user = userService.GetUser(request.Email);

            if (user == null) return NotFound("Аккаунт не найден.");

            if(user.Confirmed == true) return Ok("Аккаунт уже был активирован.");

            if (request.ActivationCode.Equals(getValidationCode(user.Email)))
            {
                // если юзер с данным email существует и код активации введён верно - активируем аккаунт.
                user.Confirmed = true;
                var result = await userService.UpdateUserAsync(user);
                if (result)
                {
                    _ = historyService.NewActivityAsync(user.Id, ActivityTypes.Activation);
                    return Ok("Аккаунт успешно активирован.");
                }
            }

            return BadRequest("Активация провалилась.");
        }

        private string getValidationCode(string value)
        {
            return hashService.Get128Hash(value).Substring(3, 10);
        }
    }
}