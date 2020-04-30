using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cantina.Models;

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
        private readonly int _messagesBufferSize;

        // шаблон для удаления html-тегов
        public Regex StripTagsPattern { get; }

        public MessageService(IServiceProvider provider, ILogger<MessageService> logger, IOptions<ApiOptions> apiOptions)
        {
            _services = provider;
            _logger = logger;
            _messagesBufferSize = (apiOptions.Value.MessagesBufferSize > 0) ? apiOptions.Value.MessagesBufferSize : 20;
            logger.LogInformation("Messages buffer size - " + _messagesBufferSize);
            var stripTagsPattern = new StringBuilder("<");
            var allowedTags = new StringBuilder();
            if (apiOptions.Value.AllowedTags.Count > 0)
            {
                stripTagsPattern.Append("(?!/?(");
                int i = 0;
                for (; i < apiOptions.Value.AllowedTags.Count - 1; i++)
                {
                    stripTagsPattern.Append($"({apiOptions.Value.AllowedTags[i]})|");
                    allowedTags.Append(apiOptions.Value.AllowedTags[i] + ", ");
                }
                stripTagsPattern.Append($"({apiOptions.Value.AllowedTags[i]})))");
                allowedTags.Append(apiOptions.Value.AllowedTags[i]);
            }
            stripTagsPattern.Append(@"[^>]*(?:\s/)?>");
            StripTagsPattern = new Regex(stripTagsPattern.ToString());
            logger.LogInformation("Allowed tags: " + allowedTags.ToString());
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
            int count = (Messages.Count > _messagesBufferSize) ? _messagesBufferSize : Messages.Count;
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
            int messagesToRemoveCount = (Messages.Count > _messagesBufferSize) ? Messages.Count - _messagesBufferSize : 0;
            lock (_locker) Messages.RemoveRange(0, messagesToRemoveCount);

            // Обновляем количество оставшихся уже сохранённых сообщений
            _savedMessages = _savedMessages + messagesToSaveCount - messagesToRemoveCount;
        }
    }
}