//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Cantina.Models;
//using Cantina.Services;

//namespace Cantina.Controllers
//{
//    /// <summary>
//    /// Контроллер выдаёт записи из истории действий юзера, которые должны отображаться в профиле.
//    /// </summary>
//    [AllowAnonymous]
//    public class HistoryController : ApiBaseController
//    {
//        /* Конкретно - запись о регистрации юзера
//         * Запись о последнем входе в чат
//         * Запись о последней смене никнейма? возможно
//         * Так же позже отдельный метод, который выдаёт всю историю действий изера (для админки)
//         */
        
//        private readonly DataContext _database;

//        public HistoryController(DataContext data)
//        {
//            _database = data;
//        }

//        [HttpGet("userId")]
//        public async Task<ActionResult> GetUserHistory(int userId)
//        {

//        }
//    }
//}
