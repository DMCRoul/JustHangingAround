using Microsoft.AspNetCore.Mvc;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("Не указан пользователь");
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Пустое сообщение");
            }

            return Ok($"{request.Username}: {request.Text}");
        }
    }
}