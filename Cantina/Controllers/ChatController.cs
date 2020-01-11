using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер выполняет вход в главную комнату чата для авторизованного посетителя. Если не авторизован - возвращает 401
    /// Так же контроллер должен уметь выполнять вход в любую комнату в будущем
    /// </summary>
    public class ChatController : ApiBaseController
    {
        // TODO: Здесь должен быть код, выполняющий "вход" в комнату

        // Проверяем что запрос /Chat работает. После реализации контроллера - убрать
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "It's Chat Controller!";
        }
    }
}