﻿using System.ComponentModel.DataAnnotations;

namespace Cantina.Models.Requests
{
    /// <summary>
    /// Модель запроса на регистрацию
    /// </summary>
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Nickname]
        public string Name { get; set; }
        [Required, Password]
        public string Password { get; set; }
    }
}
