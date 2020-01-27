using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models;

namespace Cantina.Controllers
{
    /// <summary>
    /// Основной хаб чата. Принимает и пересылает сообщения
    /// </summary>
    [Authorize]
    public class MainHub : Hub
    {
        // адрес для данного хаба
        public const string path = "/hub/main";
        // сервисы
        UsersOnlineService onlineUsers;
        ConnectionService connectionService;
        MessagesService messagesService;

        public MainHub(UsersOnlineService onlineUsers, ConnectionService connectionService, MessagesService messagesService)
        {
            // подключаем зависимости
            this.onlineUsers = onlineUsers;
            this.connectionService = connectionService;
            this.messagesService = messagesService;
            // подписываемся на событие закрытия соединения
            onlineUsers.CloseConnection += this.closeCurrentConnection;
        }

        /// <summary>
        /// Метод для отправки сообщений
        /// </summary>
        //public async Task SendMessage(MessageRequest messageRequest)
        //{

        //}
        /// <summary>
        /// Метод отправляет последние n сообщений обратившемуся к нему юзеру.
        /// </summary>
        //public async Task GetMessages(int n)
        //{

        //}

        #region Подключение - отключение
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            connectionService.UserConnect(getCurrentUserId(), Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            connectionService.UserDisconnect(getCurrentUserId(), Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing) onlineUsers.CloseConnection -= this.closeCurrentConnection;
        }
        #endregion

        #region Текущее соединение

        private int getCurrentUserId()
        {
            var ClaimId = Context.User.FindFirstValue(AuthOptions.ClaimID);
            if (!String.IsNullOrEmpty(ClaimId)) return Convert.ToInt32(ClaimId);
            else return 0;
        }

        private string getCurrentUserName()
        {
            return Context.User.Identity.Name;
        }
        /// <summary>
        /// Метод завершает соединение с заданным Id
        /// </summary>
        private void closeCurrentConnection(string connectionId)
        {
            // Если Id текущего подключения совпадает с запросом, то закрываем соединение.
            if (connectionId.Equals(Context.ConnectionId)) Context.Abort();
        }

        #endregion
    }
}
