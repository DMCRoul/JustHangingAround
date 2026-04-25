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
        private readonly IWebHostEnvironment _environment;

        public ChatController(IChatRepository chatRepository, IWebHostEnvironment environment)
        {
            _chatRepository = chatRepository;
            _environment = environment;
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

        [HttpPost("attachment")]
        public async Task<IActionResult> SendAttachment(
            [FromForm] IFormFile file,
            [FromForm] string recipient,
            [FromForm] string? text)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                return BadRequest("Получатель не выбран");
            }

            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return Unauthorized();
            }

            const long maxFileSize = 20 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest("Файл слишком большой. Максимум 20 МБ");
            }

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "uploads",
                "chat");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var attachmentUrl = $"/uploads/chat/{storedFileName}";

            var message = new ChatMessage
            {
                Username = username,
                Recipient = recipient,
                Text = string.IsNullOrWhiteSpace(text) ? "[Вложение]" : text,
                CreatedAt = DateTime.Now,
                AttachmentUrl = attachmentUrl,
                AttachmentFileName = file.FileName,
                AttachmentContentType = file.ContentType,
                AttachmentSize = file.Length
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