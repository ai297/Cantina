using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Cantina.Controllers
{
    /// <summary>
    /// Базовый класс для всех апи-контроллеров
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class ApiBaseController : ControllerBase
    {
        // Здесь выделить какой-то общий функционал для всех контроллеров
    }
}