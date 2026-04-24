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

        public Task<List<ChatMessage>> GetConversationAsync(string user1, string user2)
        {
            var messages = _messages
                .Where(m =>
                    (m.Username == user1 && m.Recipient == user2) ||
                    (m.Username == user2 && m.Recipient == user1))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return Task.FromResult(messages);
        }
    }
}