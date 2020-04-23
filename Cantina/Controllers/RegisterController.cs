using Cantina.Models.Requests;
using Cantina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер отвечает за регистрацию новых пользователей
    /// </summary>
    [AllowAnonymous]
    public class RegisterController : ApiBaseController
    {
        ILogger<RegisterController> Logger;
        UserService UserService;

        public RegisterController(UserService userService, ILogger<RegisterController> logger)
        {
            UserService = userService;
            Logger = logger;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RegisterRequest request, [FromServices] IWebHostEnvironment env)
        {
            // 1. Проверяем корректность данных в запросе
            if (!TryValidateModel(request, nameof(RegisterRequest))) return BadRequest("Некорректные данные.");

            //2. Проверка никнейма на возможность использования.
            if (UserService.CheckNameForForbidden(request.Name)) return BadRequest("Имя уже занято либо запрещено.");

            // 3. Добавлем нового юзера.
            var addedUser = await UserService.AddUserAsync(request.Email, request.Name, request.Password);
            if (addedUser == null) return BadRequest("Не удалось зарегистрироваться. Возможно на данный e-mail уже имеется зарегистрированный аккаунт.");

            // 4. Отправляем уведомление на e-mail
            // TODO: здесь отправка e-mail

            if (env.IsDevelopment()) Logger.LogInformation("Accaunt '{0}' registered with Name is '{1}'.", addedUser.Email, addedUser.Profile.Name);

            return Ok("Аккаунт успешно зарегистрирован. Необходима активация.");
        }
    }
}