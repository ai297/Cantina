using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Cantina.Services
{
    public class EmailSender
    {
        private readonly IOptions<EmailOptions> _options;

        public EmailSender(IOptions<EmailOptions> options)
        {
            _options = options;
        }

        public async Task SendEmail(string email, string subject, string message, string name = "")
        {

            var emailMessage = new MimeMessage(); 

            emailMessage.From.Add(new MailboxAddress(_options.Value.ChatName, _options.Value.Email));               //email отправителя должен совпадать с указанным в Authenticate
            emailMessage.To.Add(new MailboxAddress(name, email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) //упаковывает сообщение в html файл
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_options.Value.Server, _options.Value.Port, _options.Value.SSLEnable);    //настроки smtp сервера и безопасности. оставить как есть. (сервер, порт, SSL(true/false)) 
                await client.AuthenticateAsync(_options.Value.Email, _options.Value.Password);                      //сюда надо ввести логин и пароль от почты
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
