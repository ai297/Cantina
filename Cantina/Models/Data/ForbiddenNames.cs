using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Таблица с никнеймами, запрещёнными к использованию (включая уже занятые кем-то никнеймы)
    /// </summary>
    public class ForbiddenNames
    {
        public string Name { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
