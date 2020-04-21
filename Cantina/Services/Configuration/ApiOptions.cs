using System.Collections.Generic;

namespace Cantina.Services
{
    public class ApiOptions
    {
        /// <summary>
        /// Постоянное количество сообщений чата в буфере. Столько же сообщений увидит человек, заходя в чат.
        /// </summary>
        public int MessagesBufferSize { get; set; } = 20;
        /// <summary>
        /// Разрешенные к использованию теги
        /// </summary>
        public List<string> AllowedTags { get; set; }

        public ApiOptions()
        {
            AllowedTags = new List<string>();
        }
    }
}
