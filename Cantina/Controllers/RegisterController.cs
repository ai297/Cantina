using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public ActionResult Post([FromBody] RegisterRequest request)
        {
            // 1. Валидация данных.
            if (!TryValidateModel(request, nameof(request)))
            {
                return BadRequest(new { errorText = "Invalid data" });
            }

            // 2. Шифруем пароль.
            var hashedPassword = hashService.GetHash(request.Password);

            // 3. Создаём нового юзера.
            var profile = new UserProfile
            {
                Name = request.Name,
                Gender = request.Gender,
                Location = request.Location,
                RegisterDate = DateTime.Now
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
                database.SaveChanges();
            }
            catch
            {
                return BadRequest(new { errorText = "Database insert error" });
            }
            return Ok();
        }
    }
}