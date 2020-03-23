using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cantina.Models.Response;

namespace Cantina.Controllers
{
    /// <summary>
    /// Методы клиента, доступные серверу
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// Добавить юзера в список онлайна
        /// </summary>
        Task AddUserToOnlineList(OnlineSessionInfo sessionInfo);
        /// <summary>
        /// Удалить юзера из списка онлайн
        /// </summary>
        Task RemoveUserFromOnlineList(int userId);
        /// <summary>
        /// Метод получения и вывода сообщения
        /// </summary>
        Task ReceiveMessage(ChatMessage message);
        /// <summary>
        /// Метод выполнения произвольной команды
        /// </summary>
        Task RunCommand(string commandName, object data);
    }
}
