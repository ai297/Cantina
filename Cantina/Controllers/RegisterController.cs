using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cantina.Models.Requests;
using Cantina.Services;


namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер отвечает за регистрацию новых пользователей
    /// </summary>
    [AllowAnonymous]
    public class RegisterController : ApiBaseController
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly UserService _userService;
        private readonly EmailSender _emailSender;
        private readonly TokenGenerator _tokenGenerator;

        public RegisterController(UserService userService, EmailSender emailSender, TokenGenerator tokenGenerator, ILogger<RegisterController> logger)
        {
            _userService = userService;
            _logger = logger;
            _emailSender = emailSender;
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// Обрабатываем POST - запрос на регистрацию нового юзера
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RegisterRequest request, [FromServices] IWebHostEnvironment env, [FromServices] IOptions<AuthOptions> authOptions)
        {
            // 1. Проверяем корректность данных в запросе
            if (!TryValidateModel(request, nameof(RegisterRequest))) return BadRequest("Некорректные данные.");

            //2. Проверка никнейма на возможность использования.
            if (_userService.CheckNameForForbidden(request.Name)) return BadRequest("Имя уже занято либо запрещено.");

            // 3. Добавлем нового юзера.
            var addedUser = await _userService.AddUserAsync(request.Email, request.Name, request.Password);
            if (addedUser == null) return BadRequest("Не удалось зарегистрироваться. Возможно на данный e-mail уже имеется зарегистрированный аккаунт.");

            if (env.IsDevelopment()) _logger.LogInformation("Accaunt '{0}' registered with Name is '{1}'.", addedUser.Email, addedUser.Profile.Name);

            // 4. Отправляем уведомление на e-mail
            var mailPath = Path.Combine("Data", "Mails", "web-confirm.html");

            if (env.IsDevelopment()) _logger.LogInformation("Search accaunt confirm mail from path '{0}'...", mailPath);

            var mailFileInfo = env.ContentRootFileProvider.GetFileInfo(mailPath);
            if (mailFileInfo.Exists)
            {
                using var fileStream = mailFileInfo.CreateReadStream();
                using StreamReader streamReader = new StreamReader(fileStream);
                var mailTemplate = streamReader.ReadToEnd();
                if (!String.IsNullOrEmpty(mailTemplate))
                {
                    var token = _tokenGenerator.GetActivationToken(addedUser.Email);
                    mailTemplate = mailTemplate.Replace("{TOKEN}", token).Replace("{NAME}", addedUser.Profile.Name);
                    try
                    {
                        await _emailSender.SendEmail(addedUser.Email, addedUser.Profile.Name + ", добро пожаловать в Кантину!", mailTemplate, addedUser.Profile.Name);
                        if (env.IsDevelopment()) _logger.LogInformation("Accaunt confirm email for user '{}' was sent.", addedUser.Profile.Name);
                        return Ok($"Аккаунт успешно зарегистрирован. Теперь необходимо его активировать. На ваш email отправлено письмо с кодом для активации аккаунта. Внимание! код действует в течении {authOptions.Value.ActivationTokenLifetime} дней с текущего момента.");
                    } catch (Exception ex)
                    {
                        _logger.LogError(ex, "Accaunt confirm mail sending error");
                        return Ok("Аккаунт успешно зарегистрирован. Возникли проблемы при отправке email, пожалуйста обратитесь к администратору."); // если шаблон письма подтверждения не найден
                    }
                }
            }
            if (env.IsDevelopment()) _logger.LogWarning("Email template file was not found.");
            return Ok("Аккаунт успешно зарегистрирован. Для получения кода активации обратитесь к администратору."); // если шаблон письма подтверждения не найден
        }
    }
}