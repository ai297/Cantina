using System;
using System.ComponentModel.DataAnnotations;

namespace Cantina.Models
{
    /// <summary>
    /// Модель сообщения
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string AuthorName { get; set; }

        public int AuthorId { get; set; }

        public DateTime DateTime { get; set; }

        public MessageTypes Type { get; set; } = MessageTypes.Base;
        [Required, MaxLength(500)]
        public string Text { get; set; }

        public int[] Recipients { get; set; } = new int[0];

        public string NameStyle { get; set; }

        public string MessageStyle { get; set; }
    }
}
