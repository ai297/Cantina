using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Cantina.Services;
using Cantina.Models;
using Cantina.Models.Requests;

namespace Cantina.Controllers
{
    /// <summary>
    /// Основной хаб чата. Принимает и пересылает сообщения
    /// </summary>
    [Authorize]
    public class MainHub : Hub<IChatClient>
    {
        // адрес для данного хаба
        public const string PATH = "/hub";
        // список юзеров онлайн
        private readonly OnlineService OnlineService;

        // Id текущего юзера (из клеймов)
        int CurrentUserId { get => Convert.ToInt32(Context.UserIdentifier); }
        // роль текущего юзера (из клеймов)
        UserRoles CurrentUserRole
        {
            get
            {
                UserRoles role;
                if (Enum.TryParse<UserRoles>(Context.User.FindFirstValue(AuthOptions.Claims.Role), out role)) return role;
                else return UserRoles.User;
            }
        }
        // сессия текущего юзера (из списка онлайна)
        OnlineSession currentUser;
        OnlineSession CurrentUser { 
            get
            {
                if (currentUser == null) currentUser = OnlineService.GetSessionInfo(CurrentUserId);
                return currentUser;
            }
        }

        public MainHub(OnlineService onlineUsers, UsersHistoryService historyService)
        {
            OnlineService = onlineUsers;
        }

        #region Подключение - Отключение
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// и обавляет юзера в список онлайна
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await OnlineService.AddUser(CurrentUserId, Context.ConnectionId);
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// удаляет id текущего клиента из списка онлайна
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (CurrentUser != null) OnlineService.RemoveConnection(CurrentUserId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Правильный выход из чата
        /// Если у юзера активен один клиент - отправляем сообщение о выходе и удаляем юзера из списка у клиентов
        /// </summary>
        public async Task Exit()
        {
            await OnlineService.RemoveUser(CurrentUser.UserId);
            Context.Abort();
        }

        #endregion

        #region Сообщения
        /// <summary>
        /// Отправка сообщений всем
        /// </summary>
        public async Task SendMessage(MessageRequest messageRequest)
        {
            if (messageRequest.Text.Length < 2) return;

            // TODO: Проверка на возможность отправки сообщения юзером
            // если не админ отправляет системное сообщение - заменяем сообщение на обычное

            switch(messageRequest.MessageType)
            {
                // Приватное сообщение
                case MessageTypes.Privat:
                    // определяем список получателей, включая отправителя
                    List<string> recipients = new List<string>() { Context.UserIdentifier };
                    foreach (int id in messageRequest.Recipients) recipients.Add(id.ToString());
                    await Clients.Users(recipients).ReceiveMessage(NewMessage(messageRequest.Text, messageRequest.Recipients, messageRequest.MessageType));
                    break;
                // системные сообщения
                case MessageTypes.System:
                    if (CurrentUserRole != UserRoles.Admin)
                    {
                        messageRequest.MessageType = MessageTypes.Base;
                    }
                    goto default;
                // Общее сообщение
                default:
                    await Clients.All.ReceiveMessage(NewMessage(messageRequest.Text, messageRequest.Recipients, messageRequest.MessageType));
                    // TODO: Сделать сохранение сообщений в архиве
                    break;
            }
            // обновляем время последней активности
            CurrentUser.LastActivityTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Возвращает сообщение от текущего юзера
        /// </summary>
        private ChatMessage NewMessage(string text, int[] recipients, MessageTypes messageType = MessageTypes.Base)
        {
            return NewMessage(text, recipients, CurrentUser.UserId, CurrentUser.Name,
                messageType, CurrentUser.NameStyle, CurrentUser.MessageStyle);
        }
        /// <summary>
        /// Возвращает новое сообщение
        /// </summary>
        private ChatMessage NewMessage(string text, int[] recipients, int authorId = 0, string authorName = null,
            MessageTypes messageType = MessageTypes.Base, string nameStyle = null, string messageStyle = null)
        {
            return new ChatMessage
            {
                AuthorId = authorId,
                AuthorName = authorName,
                DateTime = DateTime.UtcNow,
                Type = messageType.ToString(),
                Text = text,
                Recipients = recipients,
                NameStyle = nameStyle,
                MessageStyle = messageStyle
            };
        }

        private ChatMessage NewSystemMessage(string text)
        {
            return new ChatMessage
            {
                AuthorId = CurrentUserId,
                AuthorName = CurrentUser.Name,
                DateTime = DateTime.UtcNow,
                Type = MessageTypes.System.ToString(),
                Text = text,
            };
        }
        #endregion
    }
}
