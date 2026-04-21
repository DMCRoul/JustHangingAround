using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private readonly List<ChatMessage> _messages = new();

        public Task AddAsync(ChatMessage message)
        {
            if (message.CreatedAt == default)
            {
                message.CreatedAt = DateTime.Now;
            }

            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<List<ChatMessage>> GetAllAsync()
        {
            return Task.FromResult(_messages.ToList());
        }
    }
}