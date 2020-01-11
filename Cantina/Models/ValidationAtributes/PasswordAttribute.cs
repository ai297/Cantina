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
            if (String.IsNullOrEmpty(password) || password.Length < 5) return false;
            else return true;
        }
    }
}
