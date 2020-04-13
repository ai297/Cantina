using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Cantina.Services;


namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер для вывода сообщений из "архива"
    /// </summary>
    public class ArchiveController : ApiBaseController
    {
        private readonly MessageService _messageService;

        public ArchiveController(MessageService messageService)
        {
            _messageService = messageService;
        }


        [HttpGet("{date}/{quantity?}/{page?}")]
        public async Task<ActionResult> GetMessages(string date, int quantity = 30, int page = 0)
        {
            DateTime archiveDate;
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out archiveDate))
            {
                var messages = await _messageService.GetMessagesFromArchive(archiveDate.Date, quantity, page);
                if (messages.Length > 0) return Ok(messages);
                else return NoContent();
            }
            else return BadRequest();
        }

    }
}