using Microsoft.AspNetCore.Mvc;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> _users = new List<User>();

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пустые данные");
            }

            var existingUser = _users.FirstOrDefault(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            var user = new User
            {
                Username = request.Username,
                Password = request.Password
            };

            _users.Add(user);

            return Ok("Пользователь зарегистрирован");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пустые данные");
            }

            var user = _users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (user.Password != request.Password)
            {
                return BadRequest("Неверный пароль");
            }

            return Ok("Вход выполнен");
        }
    }
}