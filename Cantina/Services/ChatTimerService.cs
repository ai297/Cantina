using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис работает в фоне и через заданный интервал проверяет и обновляет статусы юзеров в онлайне
    /// </summary>
    public class ChatTimerService : BackgroundService
    {
        private int ChatTimerPeriod;
        
        private readonly ILogger<ChatTimerService> _logger;
        private readonly OnlineUsersService _onlineUsers;
        private readonly MessageService _messageService;
        private Timer Timer;

        public ChatTimerService(OnlineUsersService onlineService, ILogger<ChatTimerService> logger, MessageService messageService, IConfiguration configuration)
        {
            _onlineUsers = onlineService;
            _logger = logger;
            _messageService = messageService;

            // Интервал таймера в минутах. Задаётся в appsettings, по-умолчанию - 2 минуты.
            var ConfigPeriod = configuration.GetValue<int>("ChatTimerPeriod");
            ChatTimerPeriod = (ConfigPeriod > 0) ? ConfigPeriod : 2;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Chat timer service is started with {ChatTimerPeriod} min. intervals.");

            Timer = new Timer(async (object state) =>
            {
                await _onlineUsers.CheckUsersStatus();
                await _messageService.SaveMessages();
            },
            null, TimeSpan.FromMinutes(ChatTimerPeriod), TimeSpan.FromMinutes(ChatTimerPeriod));

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Timer?.Dispose();
            base.Dispose();
        }
    }
}