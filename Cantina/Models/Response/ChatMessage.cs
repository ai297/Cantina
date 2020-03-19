using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Models.Response
{
    /// <summary>
    /// Модель сообщения
    /// </summary>
    public class ChatMessage
    {
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public DateTime DateTime { get; set; }
        public String Type { get; set; } = MessageTypes.Base.ToString();
        public string Text { get; set; }
        public List<int> Recipients { get; set; }
    }
}
