﻿using Cantina.Models.Requests;
using Cantina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Cantina.Controllers
{

    /// <summary>
    /// Контроллер авторизации. Возвращает JWT (токен авторизации) в обмен на валидные login/password
    /// </summary>
    public class AuthController : ApiBaseController
    {
        private readonly UserService _userService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly HashService _hashService;

        public AuthController(UserService userService, TokenGenerator tokenGenerator, HashService hashService)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _hashService = hashService;
        }

        /// <summary>
        /// Метод ищет юзера в бд по email и поверяет пароль. В случае успеха возвращает токен авторизации.
        /// </summary>
        /// <param name="request">Email и Password в теле запроса.</param>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetToken([FromBody] LoginRequest request)
        {
            // проверяем запрос
            if (!TryValidateModel(request, nameof(request))) return BadRequest("Некорректные данные.");
            // Ищем юзера по email и проверяем пароль.
            var user = _userService.GetUser(request.Email);
            // Если не нашли или не совпадает пароль - не авторизован.
            if (user == null || !user.PasswordEqual(_hashService.Get256Hash(request.Password, request.Email))) return BadRequest("Неверный логин или пароль");
            // Если аккаунт заблокирован
            if (!user.Active) return Forbid("Доступ запрещён");
            if (!user.Confirmed) return Ok(new { Result = "Unconfirmed" });
            // Генерируем и возвращаем токен
            var userAgent = _hashService.Get128Hash(HttpContext.Request.Headers["User-Agent"]);
            return Ok(new { Result = _tokenGenerator.GetAuthToken(user.Id, user.Email, user.Role, userAgent, request.LongLife) });
        }

        /// <summary>
        /// Метод обновляет токен авторизации
        /// </summary>
        [HttpGet("{remember?}")]
        public ActionResult GetNewToken(bool remember = false)
        {
            // получаем информацию о юзере из клэймов, сохранённых в токене
            var ClaimId = HttpContext.User.FindFirstValue(ChatConstants.Claims.ID);
            var ClaimUA = HttpContext.User.FindFirstValue(ChatConstants.Claims.UserAgent);
            var ClaimEmail = HttpContext.User.FindFirstValue(ChatConstants.Claims.Email);
            var userAgent = _hashService.Get128Hash(HttpContext.Request.Headers["User-Agent"]);

            // если не установлен claim с Id пользователя
            // или значение клэйма юзер-агента не равно текущему хэшу User-Agent (другой браузер или другое устройство)
            // то возвращаем код 401
            if (String.IsNullOrEmpty(ClaimId) ||
                String.IsNullOrEmpty(ClaimUA) ||
                !ClaimUA.Equals(userAgent)) return Unauthorized();

            // ищем юзера по Id
            var user = _userService.GetUser(Convert.ToInt32(ClaimId));
            // если юзер не найден или аккаунт не подтверждён / не активен
            if (user == null || !user.Active || !user.Confirmed) return Unauthorized();
            // если юзер изменил email то рефреш-токен не действителен
            if (!user.Email.Equals(ClaimEmail)) return Unauthorized();

            // если всё впорядке - обновляем и возвращаем оба токена
            return Ok(new { Result = _tokenGenerator.GetAuthToken(user.Id, user.Email, user.Role, userAgent, remember) });
        }
    }
}