using Microsoft.AspNetCore.Mvc;
using JustHangingAround.Repositories;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Пустые данные");
            }

            var message = new ChatMessage
            {
                Username = request.Username,
                Text = request.Text,
                CreatedAt = DateTime.Now
            };

            await _chatRepository.AddAsync(message);

            return Ok(message);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var messages = await _chatRepository.GetAllAsync();
            return Ok(messages);
        }
    }
}