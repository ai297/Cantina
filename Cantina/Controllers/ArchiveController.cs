using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Cantina.Services;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер для вывода сообщений из "архива"
    /// </summary>
    public class ArchiveController : ApiBaseController
    {
        private readonly DataContext _dataBase;

        public ArchiveController(DataContext dataContext)
        {
            _dataBase = dataContext;
        }


        /// <summary>
        /// Метод возвращает список всех дат, за которые имеется архив
        /// </summary>
        [HttpGet("{days?}")]
        public async Task<ActionResult> GetDates(int days = 60)
        {
            if (days < 1) days = 1;
            else if (days > 365) days = 365;

            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            var dates = await _dataBase.Archive.Where(msg => msg.DateTime >= startDate && msg.DateTime < DateTime.UtcNow.Date).Select(msg => msg.DateTime.Date).Distinct().ToArrayAsync();
            Array.Sort(dates);
            return Ok(dates);
        }


        /// <summary>
        /// Метод возвращает сообщения из архива за конкретную дату
        /// поддерживает постраничный вывод по quantity сообщений на странице
        /// </summary>
        [HttpGet("messages/{quantity?}/{date?}/{page?}")]
        public async Task<ActionResult> GetMessages(int quantity = 30, string date = null, int page = 0)
        {
            DateTime archiveDate;

            if(String.IsNullOrEmpty(date)) archiveDate = DateTime.UtcNow.AddDays(-1).Date;
            else if (!DateTime.TryParseExact(date, "yyyy-M-d", null, System.Globalization.DateTimeStyles.None, out archiveDate)) 
                return BadRequest("Неверный формат даты");

            if (archiveDate > DateTime.UtcNow.AddDays(-1).Date) return NotFound();
            var messages = _dataBase.Archive.Where(msg => msg.DateTime.Date == archiveDate.Date);
            if (quantity > 0) messages = messages.Skip(page * quantity).Take(quantity);         // если количество запрашиваемых сообщений > 0 - запрашиваем только нужные сообщения, иначе запрашиваем все сообщения за дату
            var result = await messages.OrderBy(msg => msg.DateTime).ToArrayAsync();
            int count;
            if (quantity > 0) count = await _dataBase.Archive.Where(msg => msg.DateTime.Date == archiveDate.Date).CountAsync();
            else count = result.Length;
            if (result.Length > 0) return Ok(new { All = count, Date = archiveDate, Messages = result });
            else return NotFound();
        }

    }
}