using System;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;


namespace Cantina.Models
{
    /// <summary>
    /// Атрибут для валидации имени юзера
    /// </summary>
    public class NicknameAttribute : ValidationAttribute
    {
        const string pattern = @"^[a-zа-я]{2,9}\s?[a-zа-я0-9]{2,10}$";  // шаблон для никнейма

        public override bool IsValid(object value)
        {
            var nickname = value.ToString();
            if (String.IsNullOrEmpty(nickname)) return false;

            var namePattern = new Regex(pattern, RegexOptions.IgnoreCase);
            return namePattern.IsMatch(nickname);
        }
    }
}
