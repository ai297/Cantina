using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantina.Models;
using Cantina.Services;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер выдаёт записи из истории действий юзера, которые должны отображаться в профиле.
    /// </summary>
    [AllowAnonymous]
    public class HistoryController : ApiBaseController
    {
        /* Конкретно - запись о регистрации юзера
         * Запись о последнем входе в чат
         * Запись о последней смене никнейма? возможно
         * Так же позже отдельный метод, который выдаёт всю историю действий изера (для админки)
         */
        
        private readonly DataContext _database;

        public HistoryController(DataContext data)
        {
            _database = data;
        }

        //[HttpGet("userId")]
        //public async Task<ActionResult> GetUserHistory(int userId)
        //{

        //}

        ///// <summary>
        ///// Метод возвращает список активностей всех юзеров в определённый день.
        ///// </summary>
        ///// <param name="date">Дата, за которую ищем активности.</param>
        ///// <param name="activityType">Тип активностей, которые ищем. если не задан - ищем любые типы активностей.</param>
        ///// <param name="quantity">Количество записей, которые возвращаем. Если -1 - возвращаем все записи.</param>
        ///// <param name="page">Множитель к количеству записей, для постраничного вывода</param>
        ///// <returns></returns>
        //public async Task<UserHistory[]> GetActivitysOfDate(DateTime date, ActivityTypes? activityType = null, int quantity = -1, int page = 0)
        //{
        //    var skip = quantity * page;
        //    var result = _database.History.Where(activity => activity.Date.Date == date.Date);
        //    if (activityType != null) result = result.Where(activity => activity.Type == activityType);
        //    if (quantity > 0) result = result.Skip(skip).Take(quantity);
        //    return await result.ToArrayAsync();
        //}
    }
}
