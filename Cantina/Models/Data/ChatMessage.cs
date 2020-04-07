using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Модель сообщения
    /// </summary>
    public class ChatMessage
    {
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public DateTime DateTime { get; set; }
        public string Type { get; set; } = MessageTypes.Base.ToString();
        public string Text { get; set; }
        public int[] Recipients { get; set; } = new int[0];
        public string NameStyle { get; set; }
        public string MessageStyle { get; set; }
    }
}
