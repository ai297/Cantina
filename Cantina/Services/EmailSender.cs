using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Cantina.Services
{
    public class EmailSender
    {

        public void SendEmail(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage(); 

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", "email-отправителя")); //email отправителя должен совпадать с указанным в Authenticate
            emailMessage.To.Add(new MailboxAddress("Пользователь:", "email-получателя"));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) //упаковывает сообщение в html файл
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.yandex.ru", 465, true);        //настроки smtp сервера и безопасности. оставить как есть. (сервер, порт, SSL(true/false)) 
                client.Authenticate("email-отправителя", "пароль"); //сюда надо ввести логин и пароль от почты
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }
    }
}
