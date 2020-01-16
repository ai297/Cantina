using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Ответ для неудачных запросов
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Сообщение клиенту
        /// </summary>
        public string Message { get; set; }
    }
}
