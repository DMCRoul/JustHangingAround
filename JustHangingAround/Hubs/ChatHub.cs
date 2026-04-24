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

        public async Task SendMessage(string recipient, string text)
        {
            var sender = Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(sender) ||
                string.IsNullOrWhiteSpace(recipient) ||
                string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var message = new ChatMessage
            {
                Username = sender,
                Recipient = recipient,
                Text = text,
                CreatedAt = DateTime.Now
            };

            await _chatRepository.AddAsync(message);

            await Clients.User(recipient).SendAsync("ReceiveMessage", message);
            await Clients.User(sender).SendAsync("ReceiveMessage", message);
        }
    }
}