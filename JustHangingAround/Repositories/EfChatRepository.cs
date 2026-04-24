using JustHangingAround.Data;
using JustHangingAround.Models;
using JustHangingAround.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JustHangingAround.Repositories
{
    public class EfChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public EfChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetAllAsync()
        {
            return await _context.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessage
                {
                    Username = m.Username,
                    Recipient = m.Recipient,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        public async Task AddAsync(ChatMessage message)
        {
            var entity = new ChatMessageEntity
            {
                Username = message.Username,
                Recipient = message.Recipient,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            };

            _context.Messages.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetConversationAsync(string user1, string user2)
        {
            return await _context.Messages
                .Where(m =>
                    (m.Username == user1 && m.Recipient == user2) ||
                    (m.Username == user2 && m.Recipient == user1))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessage
                {
                    Username = m.Username,
                    Recipient = m.Recipient,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }
    }
}