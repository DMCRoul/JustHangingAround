using JustHangingAround.Repositories;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustHangingAround.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        // Запасной HTTP-endpoint для отправки сообщения.
        // WPF-клиент сейчас отправляет сообщения через SignalR,
        // чтобы сообщения сразу приходили всем подключённым клиентам.
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Пустой текст сообщения");
            }

            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return Unauthorized();
            }

            var message = new ChatMessage
            {
                Username = username,
                Text = request.Text,
                CreatedAt = DateTime.Now
            };

            await _chatRepository.AddAsync(message);

            return Ok(message);
        }

        [HttpGet("conversation/{username}")]
        public async Task<IActionResult> GetConversation(string username)
        {
            var currentUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(currentUser))
            {
                return Unauthorized();
            }

            var messages = await _chatRepository.GetConversationAsync(currentUser, username);

            return Ok(messages);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var messages = await _chatRepository.GetAllAsync();
            return Ok(messages);
        }
    }
}