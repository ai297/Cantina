using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cantina.Models;
using Cantina.Services;

namespace Cantina.Controllers
{
    /// <summary>
    /// Контроллер возвращает информацию о посетителе. Если вызывается без параметров - возвращает текущего юзера,
    /// если указан ID - возвращает конкретного юзера
    /// </summary>
    public class UserInfoController : ApiBaseController
    {

        DataContext database;

        public UserInfoController(DataContext dataContext)
        {
            this.database = dataContext;
        }

        [HttpGet]
        public ActionResult<User> Get()
        {
            var ClaimId = HttpContext.User.FindFirst(AuthOptions.ClaimID).Value;
            if (String.IsNullOrEmpty(ClaimId)) return BadRequest("User id is not set");

            var user = GetUser(Convert.ToInt32(ClaimId));
            if (user != null) return new ObjectResult(user);
            else return NotFound();
        }

        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var user = GetUser(id);
            if (user != null) return new ObjectResult(user);
            else return NotFound();
        }

        private User GetUser(int userId)
        {
            return database.Users.SingleOrDefault<User>(u => u.Id == userId);
        }
    }
}