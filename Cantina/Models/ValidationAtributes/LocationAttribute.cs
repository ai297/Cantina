using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Cantina.Models
{
    /// <summary>
    /// Атрибут валидации расположения юзера
    /// </summary>
    public class LocationAttribute : ValidationAttribute
    {
        const string pattern = @"^([а-я\w\s]{4,32})?$";       // шаблон валидации

        public override bool IsValid(object value)
        {
            // если строка пустая - всё впорядке
            if (value == null || String.IsNullOrEmpty(value.ToString())) return true;
            // но если строка не пустая - должна соответствовать шаблону
            var locationPattern = new Regex(pattern, RegexOptions.IgnoreCase);
            return locationPattern.IsMatch(value.ToString());
        }
    }
}
