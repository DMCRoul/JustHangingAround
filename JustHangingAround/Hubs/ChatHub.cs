using JustHangingAround.Repositories;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JustHangingAround.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task SendMessage(string text)
        {
            var username = Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var message = new ChatMessage
            {
                Username = username,
                Text = text,
                CreatedAt = DateTime.Now
            };

            await _chatRepository.AddAsync(message);
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}