using Cantina.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис для хранения и работы с N последними сообщениями чата.
    /// </summary>
    public class MessageService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<MessageService> _logger;

        // кеш последних сообщений чата
        private static List<ChatMessage> Messages = new List<ChatMessage>(150);
        private static object _locker = new object();
        private static int _savedMessages = 0;
        // количество последних сообщений, выводимых юзеру
        private readonly int _maxOldMessagesToShow;

        public MessageService(IServiceProvider provider, IConfiguration config, ILogger<MessageService> logger)
        {
            _services = provider;
            _logger = logger;
            var maxOldMessagesToShow = config.GetValue<int>("MaxOldMessagesToShow");
            _maxOldMessagesToShow = (maxOldMessagesToShow > 0) ? maxOldMessagesToShow : 20;
        }

        public void AddMessage(ChatMessage message)
        {
            // лишняя проверка, что бы приватное сообщение не попало в архив - не повредит
            if (message.Type == MessageTypes.Privat) return;

            lock (_locker)
            {
                Messages.Add(message);
            }
        }

        /// <summary>
        /// Список последних сообщений в чате
        /// </summary>
        public IEnumerable<ChatMessage> GetLastMessages()
        {
            int count = (Messages.Count > _maxOldMessagesToShow) ? _maxOldMessagesToShow : Messages.Count;
            return Messages.TakeLast<ChatMessage>(count);
        }

        /// <summary>
        /// Запись сообщений из кеша в базу данных
        /// </summary>
        public async Task SaveMessages()
        {
            // Сохраняем все сообщения кроме тех, которые уже были сохранены
            int messagesToSaveCount = Messages.Count - _savedMessages;
            if (messagesToSaveCount > 0)
            {
                using var scope = _services.CreateScope();
                var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                dataBase.Archive.AddRange(Messages.GetRange(_savedMessages, messagesToSaveCount));
                try
                {
                    await dataBase.SaveChangesAsync();
                    if (env.IsDevelopment()) _logger.LogInformation($"Save {messagesToSaveCount} messages to archive.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    return;
                }
            }

            // Удаляем старые сообщения, оставляя только MaxOldMessagesToShow последних
            int messagesToRemoveCount = (Messages.Count > _maxOldMessagesToShow) ? Messages.Count - _maxOldMessagesToShow : 0;
            lock (_locker) Messages.RemoveRange(0, messagesToRemoveCount);

            // Обновляем количество оставшихся уже сохранённых сообщений
            _savedMessages = _savedMessages + messagesToSaveCount - messagesToRemoveCount;
        }


        /// <summary>
        /// Запрос к архиву за сообщениями с конкретной датой.
        /// </summary>
        /// <param name="date">Lень, за который надо найти сообщения в виде объекта DateTime.</param>
        /// <param name="quantity">Количество сообщений. Если 0 или меньше - выводим все сообщения.</param>
        /// <param name="page">"страница" - множитель количества сообщений, которые нужно пропустить. Для постраничного вывода</param>
        /// <returns></returns>
        public async Task<ChatMessage[]> GetMessagesFromArchive(DateTime? date = null, int quantity = -1, int page = 0)
        {
            var skip = page * quantity;
            if (date == null) date = DateTime.UtcNow.AddDays(-1).Date;
            using var scope = _services.CreateScope();
            var dataBase = scope.ServiceProvider.GetRequiredService<DataContext>();
            var messages = dataBase.Archive.Where(message => message.DateTime.Date == date);
            if (quantity > 0) messages = messages.Skip(skip).Take(quantity);
            return await messages.ToArrayAsync();
        }
    }
}