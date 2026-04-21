using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private readonly List<ChatMessage> _messages = new();

        public List<ChatMessage> GetAll()
        {
            return _messages.ToList();
        }

        public ChatMessage Add(string username, string text)
        {
            var message = new ChatMessage
            {
                Username = username,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            _messages.Add(message);
            return message;
        }
    }
}