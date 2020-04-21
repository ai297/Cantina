using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис работает в фоне и через заданный интервал проверяет и обновляет статусы юзеров в онлайне
    /// </summary>
    public class ChatTimerService : BackgroundService
    {

        private readonly ILogger<ChatTimerService> _logger;
        private readonly OnlineUsersService _onlineUsers;
        private readonly MessageService _messageService;
        private readonly IOptions<IntevalsOptions> _options;
        
        private Timer archiveSavingTimer;
        private Timer onlineUsersTimer;

        public ChatTimerService(OnlineUsersService onlineService, ILogger<ChatTimerService> logger, MessageService messageService, IOptions<IntevalsOptions> options)
        {
            _onlineUsers = onlineService;
            _logger = logger;
            _messageService = messageService;
            _options = options;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var archiveSavingInterval = TimeSpan.FromMinutes(_options.Value.ArchiveSaving);
            archiveSavingTimer = new Timer(async (object state) =>
            {
                await _messageService.SaveMessages();
            },
            null, archiveSavingInterval, archiveSavingInterval);
            _logger.LogInformation($"Messages will be archived with an interval of {_options.Value.ArchiveSaving} min.");
            
            var usersUpdateInterval = TimeSpan.FromMinutes(_options.Value.OnlineUsersCheck);
            var inactivityTime = TimeSpan.FromMinutes(_options.Value.InactivityTime);
            onlineUsersTimer = new Timer(async (object state) =>
            {
                await _onlineUsers.CheckUsersStatus(usersUpdateInterval, inactivityTime);
            },
            null, usersUpdateInterval, usersUpdateInterval);
            _logger.LogInformation($"Online users will be checked every {_options.Value.OnlineUsersCheck} min.");

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            archiveSavingTimer?.Dispose();
            onlineUsersTimer?.Dispose();
            base.Dispose();
        }
    }
}