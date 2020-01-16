using System;
using System.Collections.Generic;

namespace Cantina.Models.Messages
{
    /// <summary>
    /// Интерфейс описывает модель любого сообщения.
    /// </summary>
    public interface IMessage
    {
        public int Id { get; set; }

        /// <summary>
        /// Дата и время отправки сообщения.
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// ID отправителя сообщения.
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// Список получателей сообщения
        /// </summary>
        public List<int> ReceiversId { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Переменные сообщения.
        /// </summary>
        public List<string> Variables { get; set; }

        /// <summary>
        /// Стиль оформления сообщения.
        /// </summary>
        public MessageStyle Style { get; set; }

    }
}
