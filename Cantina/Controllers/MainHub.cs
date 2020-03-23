using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models;
using Cantina.Models.Response;
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

        // сервисы
        OnlineService OnlineList;
        UserService UserService;

        OnlineSession currentUser;
        OnlineSession CurrentUser { 
            get
            {
                if (currentUser == null) currentUser = OnlineList.GetUserIfOnline(CurrentUserId);
                return currentUser;
            }
        }
        int CurrentUserId { get => Convert.ToInt32(Context.UserIdentifier); }

        //MessagesService messagesService;

        public MainHub(OnlineService onlineUsers, UserService userService, UsersHistoryService historyService)
        {
            // подключаем зависимости
            OnlineList = onlineUsers;
            UserService = userService;
        }

        #region Подключение - Отключение
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // регистрация юзера в списке онлайна
            if(OnlineList.AddUser(CurrentUserId, Context.ConnectionId, UserService))
            {
                // оповещение о входе, если это новый юзер в списке
                await Clients.All.ReceiveMessage(NewSystemMessage("В Кантину заходит <0>."));
                // добавление юзера в список онлайна на клиенте всем, кроме текущего юзера
                await Clients.Others.AddUserToOnlineList(OnlineSessionInfo.OnlineSessionOut(CurrentUser));
            }
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //TODO: Сделать проверку - отключился юзер сам или вылетел
            
            // удаление юзера из списка онлайн
            if (CurrentUser != null && OnlineList.RemoveUser(CurrentUserId, Context.ConnectionId))
            {
                // оповещение о выходе, если у юзера не осталось подключенных клиентов
                await Clients.All.ReceiveMessage(NewSystemMessage("<0> покидает Кантину."));
                // удаляем юзера из списке онлайна на всех клиентах
                await Clients.All.RemoveUserFromOnlineList(CurrentUserId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Сообщения
        /// <summary>
        /// Отправка сообщений всем
        /// </summary>
        public async Task SendMessage(MessageRequest messageRequest)
        {

            // TODO: Проверка на возможность отправки сообщения юзером

            switch(messageRequest.MessageType)
            {
                // Приватное сообщение
                case MessageTypes.Privat:
                    // определяем список получателей, включая отправителя
                    List<string> recipients = new List<string>() { Context.UserIdentifier };
                    foreach (int id in messageRequest.Recipients) recipients.Add(id.ToString());
                    await Clients.Users(recipients).ReceiveMessage(NewMessageFromResquest(messageRequest));
                    break;
                // Общее сообщение
                default:
                    await Clients.All.ReceiveMessage(NewMessageFromResquest(messageRequest));
                    // TODO: Сделать сохранение сообщений в архиве
                    break;
            }
        }

        private ChatMessage NewMessageFromResquest(MessageRequest messageRequest)
        {
            return new ChatMessage
            {
                AuthorId = CurrentUserId,
                AuthorName = CurrentUser.User.Name,
                DateTime = DateTime.UtcNow,
                Type = messageRequest.MessageType.ToString(),
                Text = messageRequest.Text,
                Recipients = messageRequest.Recipients
            };
        }

        private ChatMessage NewSystemMessage(string text)
        {
            return new ChatMessage
            {
                AuthorId = CurrentUserId,
                AuthorName = CurrentUser.User.Name,
                DateTime = DateTime.UtcNow,
                Type = MessageTypes.System.ToString(),
                Text = text,
            };
        }

        #endregion


    }
}
