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

        // POST api/users/create
        [HttpPost("create")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Login) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                string.IsNullOrWhiteSpace(user.Name) ||
                user.Password.Trim().Length < 8)
            {
                return BadRequest("Логин, Имя обязательны, Пароль обязателен и должен быть не менее 8 символов");
            }

            try
            {
                var usr = await _userService.CreateUser(user.Login, user.Password, user.Name, user.TelegramId);
                var usrDto = new UserDtoNoAuth
                {
                    Id = usr.Id,
                    Name = usr.Name,
                    TelegramId = usr.TelegramId,
                    Role = usr.Role,
                    Banned = usr.Banned
                };
                return Ok(usrDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/users/get_all
        [HttpGet("get_all")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();

            if (!users.Any())
                return NotFound("Пользователи не найдены");

            return Ok(users);
        }

        // GET api/users/get_by_id/{id}
        [HttpGet("get_by_id/{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
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
        public async Task<ActionResult<IEnumerable<UserDtoNoAuth>>> GetUsersByName(string namePart)
        {
            var users = await _userService.GetUsersByName(namePart);
            if (!users.Any())
                return NotFound($"Пользователи с именем, содержащим '{namePart}', не найдены");

            return Ok(users);
        }

        // PUT api/users/ban/{id}
        [HttpPut("ban/{id}")]
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