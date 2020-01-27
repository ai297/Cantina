using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cantina.Models
{
    /// <summary>
    /// История совершаемых действий, таких как регистрация, визит в чат или смена ника
    /// </summary>
    public class UserHistory
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Тип действия
        /// </summary>
        [Required]
        public ActivityTypes Type { get; set; }

        /// <summary>
        /// Описание действия / дополнительная информация
        /// </summary>
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public int UserID { get; set; }
        /// <summary>
        /// Навигационное свойство для связи с таблицей Users
        /// </summary>
        public virtual User User { get; set; }
    }
}
