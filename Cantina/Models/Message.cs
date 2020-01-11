using System;

namespace Cantina.Models
{
    /// <summary>
    /// Модель данных для сообщений. Сообщения не сохраняются в БД напрямую, а пересылаются через хаб. Обработку и храниение сообщений 
    /// должен обеспечивать сервис сообщений
    /// </summary>
    public class Message
    {
        public long Id { get; set; }

        /// <summary>
        /// Тип сообщения
        /// </summary>
        public MessageTypes Type { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// ID отправителя сообщения
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// Никнейм отправителя (на момент отправки)
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// ID получателя сообщения
        /// </summary>
        public int RecipientId { get; set; }

        /// <summary>
        /// Тело сообщения - текст или данные
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Оформление сообщения
        /// </summary>
        public MessageStyle Style { get; set; }
    }
}
