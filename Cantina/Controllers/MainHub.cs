using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Cantina.Models;
using Cantina.Models.Requests;
using Cantina.Services;

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
        // сервисы
        private readonly OnlineUsersService _onlineUsers;
        private readonly MessageService _messageService;
        private readonly ILogger<MainHub> _logger;

        // Id текущего юзера (из клеймов)
        private int CurrentUserId { get => Convert.ToInt32(Context.UserIdentifier); }
        // роль текущего юзера (из клеймов)
        private UserRoles CurrentUserRole
        {
            get
            {
                UserRoles role;
                if (Enum.TryParse<UserRoles>(Context.User.FindFirstValue(AuthOptions.Claims.Role), out role)) return role;
                else return UserRoles.User;
            }
        }
        // сессия текущего юзера (из списка онлайна)
        private OnlineSession _currentUser;
        private OnlineSession CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = _onlineUsers.GetSessionInfo(CurrentUserId);
                return _currentUser;
            }
        }

        // шаблон для удаления всех тегов из текста сообщения, кроме разрешенных

        public MainHub(OnlineUsersService onlineUsers, MessageService messageService, ILogger<MainHub> logger)
        {
            _onlineUsers = onlineUsers;
            _messageService = messageService;
            _logger = logger;
        }

        #region Подключение - Отключение
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// и обавляет юзера в список онлайна
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // отправляем клиенту n-последних сообщений в чате
            var lastMessages = _messageService.GetLastMessages();
            foreach (var message in lastMessages)
            {
                await Clients.Caller.ReceiveMessage(message);
            }
            // регистрируем клиента / юзера в списке онлайна
            await _onlineUsers.AddUser(CurrentUserId, Context.ConnectionId);
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// удаляет id текущего клиента из списка онлайна
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (CurrentUser != null) _onlineUsers.RemoveConnection(CurrentUserId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Правильный выход из чата
        /// Если у юзера активен один клиент - отправляем сообщение о выходе и удаляем юзера из списка у клиентов
        /// </summary>
        public async Task Exit()
        {
            await _onlineUsers.RemoveUser(CurrentUser.UserId);
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


            switch (messageRequest.MessageType)
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
                    var message = NewMessage(messageRequest.Text, messageRequest.Recipients, messageRequest.MessageType);
                    // отправляем сообщение в кеш для архива и рассылаем по клиентам
                    _messageService.AddMessage(message);
                    await Clients.All.ReceiveMessage(message);
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
            text = _messageService.StripTagsPattern.Replace(text, String.Empty);

            return new ChatMessage
            {
                AuthorId = CurrentUser.UserId,
                AuthorName = CurrentUser.Name,
                DateTime = DateTime.UtcNow,
                Type = messageType,
                Text = text,
                Recipients = recipients,
                NameStyle = CurrentUser.NameStyle,
                MessageStyle = CurrentUser.MessageStyle
            };
        }


        #endregion
    }
}
