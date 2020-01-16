using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cantina.Models
{
    /// <summary>
    /// Группы юзеров. Могут создаваться самими юзерами. Что-то типа кланов в мморпг.
    /// 
    /// пока в процессе разработки
    /// 
    /// </summary>
    public class Group
    {
        public int Id { get; set; }

        /// <summary>
        /// Название группы. К названию такие же требования, как к никнейму юзеров.
        /// </summary>
        [Required, Nickname]
        [MaxLength(30)]
        public string Caption { get; set; }

        /// <summary>
        /// Описание группы.
        /// </summary>
        [MaxLength(128)]
        public string Description { get; set; }

        /// <summary>
        /// Руководитель группы.
        /// </summary>
        [Required]
        public int LeaderId { get; set; }

        /// <summary>
        /// Список участников группы.
        /// </summary>
        public virtual List<User> Members { get; set; }
    }
}
