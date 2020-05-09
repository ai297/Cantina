using System.Threading.Tasks;

namespace Cantina.Models
{
    /// <summary>
    /// Методы клиента, доступные серверу
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// Добавить юзера в список онлайна
        /// </summary>
        Task AddUserToOnlineList(OnlineSession session);
        /// <summary>
        /// Удалить юзера из списка онлайн
        /// </summary>
        Task RemoveUserFromOnlineList(int userId);
        /// <summary>
        /// Метод получения и вывода сообщения
        /// </summary>
        Task ReceiveMessage(ChatMessage message);
        /// <summary>
        /// Хук окончания загрузки "старых" сообщений
        /// </summary>
        Task MessagesLoaded();
    }
}
