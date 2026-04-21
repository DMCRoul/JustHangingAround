using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private readonly List<ChatMessage> _messages = new();

        public void Add(ChatMessage message)
        {
            if (message.CreatedAt == default)
            {
                message.CreatedAt = DateTime.Now;
            }

            _messages.Add(message);
        }

        public List<ChatMessage> GetAll()
        {
            return _messages;
        }
    }
}