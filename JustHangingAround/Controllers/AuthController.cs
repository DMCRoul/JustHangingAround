using Microsoft.AspNetCore.Mvc;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пустые данные");
            }

            return Ok("Пользователь зарегистрирован");
        }
    }
}