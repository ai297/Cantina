using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Cantina.Controllers
{
    /// <summary>
    /// Стартовый контроллер. возвращает необходимую информацию для главной страницы (для авторизованных пользователей)
    /// </summary>
    [AllowAnonymous]
    public class MainController : ApiBaseController
    {
        public MainController()
        {
            // внедряем зависимости тут
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok("Main page for autorized users");
        }
    }
}