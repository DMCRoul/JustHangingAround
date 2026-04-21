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
            _chatRepository.Add(message);

            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}