using JustHangingAround.Repositories;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace JustHangingAround.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task SendMessage(ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Username) || string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            if (message.CreatedAt == default)
            {
                message.CreatedAt = DateTime.Now;
            }

            await _chatRepository.AddAsync(message);
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}