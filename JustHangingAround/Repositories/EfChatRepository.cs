using JustHangingAround.Data;
using JustHangingAround.Models;
using JustHangingAround.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JustHangingAround.Repositories
{
    public class EfChatRepository : IChatRepository
    {
        private readonly AppDbContext _dbContext;

        public EfChatRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ChatMessage message)
        {
            if (message.CreatedAt == default)
            {
                message.CreatedAt = DateTime.Now;
            }

            var entity = new ChatMessageEntity
            {
                Username = message.Username,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            };

            _dbContext.Messages.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetAllAsync()
        {
            return await _dbContext.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessage
                {
                    Username = m.Username,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }
    }
}