using System;
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
        DataContext database;
        IHashService hashService;

        public RegisterController(DataContext db, IHashService hashService)
        {
            this.database = db;
            this.hashService = hashService;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RegisterRequest request)
        {
            // 1. Проверяем корректность данных в запросе
            if (!TryValidateModel(request, nameof(request)))
            {
                return BadRequest(new ErrorResponse { Message = "Некорректные данные" });
            }

            // 2. Шифруем пароль.
            var hashedPassword = hashService.GetHash(request.Password);

            // 3. Создаём нового юзера.
            var profile = new UserProfile
            {
                Name = request.Name,
                Gender = request.Gender,
                Location = request.Location,
                RegisterDate = DateTime.UtcNow
            };
            var user = new User
            {
                Email = request.Email,
                Profile = profile
            };
            user.SetPasswordHash(hashedPassword.Item1, hashedPassword.Item2);

            // 4. Сохраняем в базу
            try
            {
                database.Users.Add(user);
                await database.SaveChangesAsync();
            }
            catch
            {
                return BadRequest(new ErrorResponse { Message = "Ошибка записи в базу данных" });
            }
            return Ok();
        }
    }
}