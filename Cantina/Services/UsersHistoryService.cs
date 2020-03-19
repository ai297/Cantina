using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cantina.Models;

namespace Cantina.Services
{
    /// <summary>
    /// Сервис следит за важными действиями юзера и сохраняет историю в бд
    /// </summary>
    public class UsersHistoryService
    {
        DataContext database;

        public UsersHistoryService(DataContext context)
        {
            this.database = context;
        }
        // Добавление в базу данных новой записи о действиях юзера
        public async Task NewActivityAsync(int userId, ActivityTypes activityType, string description = "")
        {
            var activity = new UserHistory
            {
                Date = DateTime.Now,
                Type = activityType,
                UserID = userId,
                Description = description
            };
            await addActivity(activity);
        }

        private async Task<bool> addActivity(UserHistory activity)
        {
            database.History.Add(activity);
            var added = await database.SaveChangesAsync();
            return added > 0;
        }
    }
}
