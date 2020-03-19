using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Cantina.Services;
using Cantina.Models.Response;

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

        //MessagesService messagesService;

        public MainHub(OnlineService onlineUsers, UserService userService, UsersHistoryService historyService)
        {
            // подключаем зависимости
            OnlineList = onlineUsers;
            UserService = userService;
        }


        ///// <summary>
        ///// Метод отправляет последние n сообщений обратившемуся к нему юзеру.
        ///// </summary>
        ////public async Task GetMessages(int n)
        ////{

        ////}

        //#region Подключение - отключение
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var uid = Convert.ToInt32(Context.UserIdentifier);
            
            // регистрация юзера в списке онлайна и оповещение о входе
            if(OnlineList.AddUser(uid, Context.ConnectionId, UserService))
            {
                var userOnline = OnlineList.GetUserIfOnline(uid);
                await Clients.All.ReceiveMessage(new ChatMessage
                {
                    AuthorId = uid,
                    AuthorName = userOnline.User.Name,
                    DateTime = DateTime.UtcNow,
                    Type = MessageTypes.System.ToString(),
                    Text = "В Кантину заходит <0>."
                });
            }
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if(disposing) onlineUsers.CloseConnection -= this.closeCurrentConnection;
        //}
        //#endregion

        //#region Текущее соединение

        //private int getCurrentUserId()
        //{
        //    var ClaimId = Context.User.FindFirstValue(AuthOptions.ClaimID);
        //    if (!String.IsNullOrEmpty(ClaimId)) return Convert.ToInt32(ClaimId);
        //    else return 0;
        //}

        //private string getCurrentUserName()
        //{
        //    return Context.User.Identity.Name;
        //}
        ///// <summary>
        ///// Метод завершает соединение с заданным Id
        ///// </summary>
        //private void closeCurrentConnection(string connectionId)
        //{
        //    // Если Id текущего подключения совпадает с запросом, то закрываем соединение.
        //    if (connectionId.Equals(Context.ConnectionId)) Context.Abort();
        //}

        //#endregion
    }
}
