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
    public class VisitsController : ApiBaseController
    {
        private readonly HistoryService _historyService;

        public VisitsController(HistoryService historyService)
        {
            _historyService = historyService;
        }


        /// <summary>
        /// Метод возвращает список всех визитов за конкретную дату. Поддерживается постраничный вывод.
        /// </summary>
        [HttpGet("{date?}/{quantity?}/{page?}")]
        public async Task<ActionResult> GetMessages(string date, int quantity = -1, int page = 0)
        {
            DateTime historyDate;
            if (String.IsNullOrEmpty(date)) historyDate = DateTime.UtcNow;
            else if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out historyDate)) return BadRequest();

            var visits = await _historyService.GetActivitysOfDate(historyDate.Date, ActivityTypes.Visit, quantity, page);
            return Ok(visits);
        }

    }
}