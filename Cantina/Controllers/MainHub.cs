using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Models;
using Cantina.Services;
using Cantina.Models.Messages;

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

        UsersOnlineService onlineUsers;
        ICacheService<User> userCacheService;

        public MainHub(UsersOnlineService onlineUsers, ICacheService<User> userCacheService)
        {
            // подключаем зависимости
            this.onlineUsers = onlineUsers;                 // список юзеров онлайн
            this.userCacheService = userCacheService;       // сервис кеширования юзеров

            // подписываемся на событие закрытия соединения
            onlineUsers.CloseConnection += this.closeCurrentConnection;
        }
        
        /// <summary>
        /// Метод срабатывает при подключении нового клиента
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // получаем Id текущего юзера.
            var userId = getCurrentUserId();
            // получаем юзера из кеша или из базы данных
            var user = await userCacheService.Get(userId);
            // если юзер найден
            if(user != null)
            {
                onlineUsers.UserConnect(user, Context.ConnectionId);    // добавляем в список онлайна
            }
        }

        /// <summary>
        /// Метод срабатывает при отключении клиента
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {

        }


        public async Task PublicMessage(PublicMessage message)
        {
            // 1. Проверка разрешений юзера на выполнение команды
            // 2. Формирование сообщения
            // 3. Рассылка сообщения получателям
            // 4. Запись в архив.
        }

        /// <summary>
        /// Приватное сообщение пересылается конкретному посетителю, не отправляется в архив.
        /// </summary>
        public async Task PrivateMessage(PrivateMessage message)
        {

        }

        /// <summary>
        /// Системное сообщение, пересылается всем посетителям.
        /// </summary>
        public async Task SystemMessage(SystemMessage message)
        {

        }

        /// <summary>
        /// Метод получает Id текущего аторизованного юзера из контекста.
        /// </summary>
        private int getCurrentUserId()
        {
            var ClaimId = Context.User.FindFirstValue(AuthOptions.ClaimID);
            if (!String.IsNullOrEmpty(ClaimId)) return Convert.ToInt32(ClaimId);
            else return 0;
        }
        /// <summary>
        /// Метод завершает соединение с заданным Id
        /// </summary>
        private void closeCurrentConnection(string connectionId)
        {
            // Если Id текущего подключения совпадает с запросом, то закрываем соединение.
            if (connectionId.Equals(Context.ConnectionId)) Context.Abort();
        }
    }
}
