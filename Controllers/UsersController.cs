using gutv_booker.Services;
using gutv_booker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost("create")]
        public async Task<ActionResult<UserDtoNoAuth>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.Password.Length < 8)
            {
                return BadRequest("Логин и имя обязательны. Пароль обязателен и минимум 8 символов.");
            }

            try
            {
                var userDto = await _userService.CreateUserAsync(request.Login, request.Password, request.Name,
                    request.TelegramId);
                return Ok(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class CreateUserRequest
        {
            public string Login { get; set; } = "";
            public string Password { get; set; } = "";
            public string Name { get; set; } = "";
            public string TelegramId { get; set; } = "";
        }

        // GET api/users/get_all
        [HttpGet("get_all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDtoNoAuth>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            if (!users.Any())
                return NotFound("Пользователи не найдены");

            return Ok(users);
        }

        // GET api/users/get_by_id/{id}
        [HttpGet("get_by_id/{id}")]
        [Authorize]
        public async Task<ActionResult<UserDtoNoAuth>> GetUserById(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя.");

            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok(user);
        }

        // GET api/users/get_by_name/{namePart}
        [HttpGet("get_by_name/{namePart}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDtoNoAuth>>> GetUsersByName(string namePart)
        {
            var users = await _userService.GetUsersByName(namePart);
            if (!users.Any())
                return NotFound($"Пользователи с именем, содержащим '{namePart}', не найдены");

            return Ok(users);
        }

        // PUT api/users/ban/{id}
        [HttpPut("ban/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BanUser(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя");

            var success = await _userService.BanUser(id);
            if (!success)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok($"Пользователь с ID {id} заблокирован");
        }

        // PUT api/users/unban/{id}
        [HttpPut("unban/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UnbanUser(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя");

            var success = await _userService.UnbanUser(id);
            if (!success)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok($"Пользователь с ID {id} разблокирован");
        }

        // PUT api/users/make_admin/{id}
        [HttpPut("make_admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MakeAdmin(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя");

            var success = await _userService.MakeAdmin(id);
            if (!success)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok($"Пользователь с ID {id} теперь администратор");
        }

        // PUT api/users/make_user/{id}
        [HttpPut("make_user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MakeUser(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя");

            var success = await _userService.MakeUser(id);
            if (!success)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok($"Пользователь с ID {id} теперь обычный пользователь");
        }

        // DELETE api/users/delete/{id}
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest("Некорректный ID пользователя");

            var success = await _userService.DeleteUser(id);
            if (!success)
                return NotFound($"Пользователь с ID {id} не найден");

            return Ok($"Пользователь с ID {id} успешно удалён");
        }
    }
}