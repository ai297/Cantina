using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Services;
using Cantina.Models;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер отвечает за регистрацию новых пользователей
    /// </summary>
    [AllowAnonymous]
    public class RegisterController : ApiBaseController
    {
        UserService userService;

        public RegisterController(DataContext db, UserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RegisterRequest request)
        {
            // 1. Проверяем корректность данных в запросе
            // TODO: Сделать более строгую проверку на недопустимые значения?
            if (!TryValidateModel(request, nameof(request)))
            {
                return BadRequest(new ErrorResponse { Message = "Некорректные данные." });
            }
            // 2. Добавлем нового юзера.
            var added = await userService.NewUser(request);
            if(!added) return BadRequest(new ErrorResponse { Message = "Имя или e-mail уже зарегистрированы." });
            else return Ok();
        }
        
    }
}