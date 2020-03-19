using System;
using System.ComponentModel.DataAnnotations;

namespace Cantina.Models
{
    /// <summary>
    /// Атрибут для валидации пароля
    /// </summary>
    public class PasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var password = value.ToString();
            return !(String.IsNullOrEmpty(password) || password.Length < 6);
        }
    }
}
