﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace Cantina.Models
{
    /// <summary>
    /// Атрибут для валидации имени юзера
    /// </summary>
    public class NicknameAttribute : ValidationAttribute
    {
        const string pattern = @"^[a-zа-я]{2,11}\s?[a-zа-я0-9]{2,11}$";     // шаблон для никнейма

        public override bool IsValid(object value)
        {
            var nickname = value.ToString();
            if (String.IsNullOrEmpty(nickname)) return false;

            var namePattern = new Regex(pattern, RegexOptions.IgnoreCase);
            return namePattern.IsMatch(nickname) && nickname.Length <= 18;  // никнейм должен соответствоввать шаблону и не превышать 18 символов в длину
        }
    }
}
