using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models.Requests;
using Microsoft.Extensions.Logging;

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
        UsersHistoryService HistoryService;

        public RegisterController(UserService userService, UsersHistoryService historyService, ILogger<RegisterController> logger)
        {
            UserService = userService;
            HistoryService = historyService;
            Logger = logger;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RegisterRequest request)
        {
            // 1. Проверяем корректность данных в запросе
            if (!TryValidateModel(request, nameof(RegisterRequest))) return BadRequest("Некорректные данные.");

            //2. Проверка никнейма на возможность использования.
            if (UserService.CheckNameForForbidden(request.Name)) return BadRequest("Имя уже занято либо запрещено.");

            // 3. Добавлем нового юзера.
            var addedUser = await UserService.AddUserAsync(request.Email, request.Name, request.Password);
            if(addedUser == null) return BadRequest("Не удалось зарегистрироваться. Возможно на данный e-mail уже имеется зарегистрированный аккаунт.");
            // 4. Запись в историю о регистрации.
            await HistoryService.NewActivityAsync(addedUser.Id, ActivityTypes.Register, $"Имя при регистрации - {addedUser.Profile.Name}");
            // Лог о регистрации в консоли сервера
            Logger.LogInformation("Accaunt '{0}' registered with Name is '{1}'.", addedUser.Email, addedUser.Profile.Name);
            
            return Ok("Аккаунт успешно зарегистрирован. Необходима активация.");
        }
    }
}