using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public class OnlineUsersMonitor : BackgroundService
    {
        const int CheckStatusPeriod = 2;
        
        private readonly ILogger<OnlineUsersMonitor> Logger;
        private readonly OnlineService OnlineUsers;
        private Timer Timer;

        public OnlineUsersMonitor(OnlineService onlineService, ILogger<OnlineUsersMonitor> logger)
        {
            OnlineUsers = onlineService;
            Logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Online Users Monitor service is started.");
            Timer = new Timer(async (object state) => await OnlineUsers.CheckUsersStatus(), null, TimeSpan.FromMinutes(CheckStatusPeriod), TimeSpan.FromMinutes(CheckStatusPeriod));
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Timer?.Dispose();
            base.Dispose();
        }
    }
}