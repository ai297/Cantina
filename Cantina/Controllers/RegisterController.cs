using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models.Requests;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер отвечает за регистрацию новых пользователей
    /// </summary>
    [AllowAnonymous]
    public class RegisterController : ApiBaseController
    {
        UserService userService;

        public RegisterController(UserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public ActionResult Post([FromBody] RegisterRequest request)
        {
            // 1. Проверяем корректность данных в запросе
            if (!TryValidateModel(request, nameof(request))) return BadRequest("Некорректные данные.");

            //2. Проверка никнейма на возможность использования.
            if (userService.CheckNameForForbidden(request.Name)) return BadRequest("Имя уже занято либо запрещено.");

            // 3. Добавлем нового юзера.
            var added = userService.NewUser(request);
            if(!added) return BadRequest("Не удалось зарегистрироваться. Возможно на данный e-mail уже имеется зарегистрированный аккаунт.");
            return Ok("Аккаунт успешно зарегистрирован. Необходима активация.");
        }
    }
}