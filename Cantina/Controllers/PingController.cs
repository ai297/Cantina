using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер для проверки доступности сервера.
    /// </summary>
    [AllowAnonymous]
    public class PingController: ApiBaseController
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }
    }
}
