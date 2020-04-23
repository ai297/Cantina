using Cantina.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер для вывода сообщений из "архива"
    /// </summary>
    public class VisitsController : ApiBaseController
    {


        //public VisitsController(HistoryService historyService)
        //{
        //    _historyService = historyService;
        //}


        ///// <summary>
        ///// Метод возвращает список всех визитов за конкретную дату. Поддерживается постраничный вывод.
        ///// </summary>
        //[HttpGet("{date?}/{quantity?}/{page?}")]
        //public async Task<ActionResult> GetMessages(string date, int quantity = -1, int page = 0)
        //{
        //    DateTime historyDate;
        //    if (String.IsNullOrEmpty(date)) historyDate = DateTime.UtcNow;
        //    else if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out historyDate)) return BadRequest();

        //    var visits = await _historyService.GetActivitysOfDate(historyDate.Date, ActivityTypes.Visit, quantity, page);
        //    return Ok(visits);
        //}

    }
}