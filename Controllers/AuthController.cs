using Microsoft.AspNetCore.Mvc;
using gutv_booker.Services;

namespace gutv_booker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetByLoginAsync(request.Login);
            if (user == null || !user.CheckPassword(request.Password))
                return Unauthorized("Неверный логин или пароль");

            if (user.Banned)
                return Unauthorized("Пользователь заблокирован");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (!user.Osnova && user.JoinDate.AddYears(1) <= today)
            {
                user.Osnova = true;
                await _userService.UpdateUserAsync(user);
            }

            var accessToken = _authService.GenerateAccessToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            await _userService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return Ok(new { accessToken, refreshToken });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var user = await _userService.GetByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
                return Unauthorized("Недействительный refresh токен");

            var newAccessToken = _authService.GenerateAccessToken(user);
            var newRefreshToken = _authService.GenerateRefreshToken();

            await _userService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = "";
    }
}