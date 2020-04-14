using Cantina.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис следит за важными действиями юзера и сохраняет историю в бд
    /// </summary>
    public class HistoryService
    {
        private readonly DataContext _database;
        private readonly ILogger<HistoryService> _logger;


        public HistoryService(DataContext context, ILogger<HistoryService> logger)
        {
            _database = context;
            _logger = logger;
        }


        /// <summary>
        /// Метод сохраняет в базе новую запись об активности юзера
        /// </summary>
        public async Task<bool> NewActivityAsync(int userId, ActivityTypes activityType, string description = "")
        {
            var activity = new UserHistory
            {
                Date = DateTime.Now,
                Type = activityType,
                UserID = userId,
                Description = description
            };
            _database.History.Add(activity);

            try
            {
                var added = await _database.SaveChangesAsync();
                return added > 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return false;
            }
        }


        /// <summary>
        /// Метод возвращает список активностей всех юзеров в определённый день.
        /// </summary>
        /// <param name="date">Дата, за которую ищем активности.</param>
        /// <param name="activityType">Тип активностей, которые ищем. если не задан - ищем любые типы активностей.</param>
        /// <param name="quantity">Количество записей, которые возвращаем. Если -1 - возвращаем все записи.</param>
        /// <param name="page">Множитель к количеству записей, для постраничного вывода</param>
        /// <returns></returns>
        public async Task<UserHistory[]> GetActivitysOfDate(DateTime date, ActivityTypes? activityType = null, int quantity = -1, int page = 0)
        {
            var skip = quantity * page;
            var result = _database.History.Where(activity => activity.Date.Date == date.Date);
            if (activityType != null) result = result.Where(activity => activity.Type == activityType);
            if (quantity > 0) result = result.Skip(skip).Take(quantity);
            return await result.ToArrayAsync();
        }
    }
}