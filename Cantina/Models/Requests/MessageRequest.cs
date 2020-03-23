using System.ComponentModel.DataAnnotations;

namespace Cantina.Models.Requests
{
    /// <summary>
    /// Запрос для отправки сообщения
    /// </summary>
    public class MessageRequest
    {
        [Required, MaxLength(512)]
        public string Text { get; set; }
        public MessageTypes MessageType { get; set; } = MessageTypes.Base;
        public int[] Recipients { get; set; } = new int[0];
    }
}