using gutv_booker.Services;
using gutv_booker.Models;
using Microsoft.AspNetCore.Mvc;

namespace gutv_booker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // POST api/users?telegramId=12345
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromQuery] string telegramId)
        {
            var user = await _userService.CreateUser(telegramId);
            return Ok(user);
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUser(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT api/users/ban/{id}
        [HttpPut("ban/{id}")]
        public async Task<ActionResult> BanUser(int id)
        {
            var success = await _userService.BanUser(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT api/users/unban/{id}
        [HttpPut("unban/{id}")]
        public async Task<ActionResult> UnbanUser(int id)
        {
            var success = await _userService.UnbanUser(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT api/users/makeadmin/{id}
        [HttpPut("makeadmin/{id}")]
        public async Task<ActionResult> MakeAdmin(int id)
        {
            var success = await _userService.MakeAdmin(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT api/users/makeuser/{id}
        [HttpPut("makeuser/{id}")]
        public async Task<ActionResult> MakeUser(int id)
        {
            var success = await _userService.MakeUser(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // GET api/users/checkadmin/{id}
        [HttpGet("checkadmin/{id}")]
        public async Task<ActionResult<bool>> CheckAdmin(int id)
        {
            var isAdmin = await _userService.CheckAdmin(id);
            return Ok(isAdmin);
        }
    }
}
