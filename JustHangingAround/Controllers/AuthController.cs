using JustHangingAround.Data;
using JustHangingAround.Models;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustHangingAround.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AuthController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пустые данные");
            }

            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            var user = new UserEntity
            {
                Username = request.Username,
                PasswordHash = request.Password
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Пользователь зарегистрирован");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пустые данные");
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (user.PasswordHash != request.Password)
            {
                return BadRequest("Неверный пароль");
            }

            return Ok("Вход выполнен");
        }
    }
}