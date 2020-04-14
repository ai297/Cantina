using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cantina.Controllers
{
    /// <summary>
    /// Web-страница сервера для проверки его доступности.
    /// </summary>
    [Route("")]
    public class DefaultController : Controller
    {
        [Route("")]
        public IActionResult Index([FromServices] IConfiguration configuration)
        {
            return View("Index", configuration.GetValue<string>("ServerVersion"));
        }

        /// <summary>
        /// метод для api проверки доступности сервера (возвращает пустой ответ с http кодом 200)
        /// </summary>
        [Route("status")]
        [HttpGet]
        public ActionResult Status()
        {
            return Ok();
        }
    }
}
