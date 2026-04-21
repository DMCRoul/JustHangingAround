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
        public IActionResult Send([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Пустые данные");
            }

            var message = new ChatMessage
            {
                Username = request.Username,
                Text = request.Text,
                CreatedAt = DateTime.Now
            };

            _chatRepository.Add(message);

            return Ok(message);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var messages = _chatRepository.GetAll();
            return Ok(messages);
        }
    }
}