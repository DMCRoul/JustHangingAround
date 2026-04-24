using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public interface IChatRepository
    {
        Task AddAsync(ChatMessage message);
        Task<List<ChatMessage>> GetAllAsync();
        Task<List<ChatMessage>> GetConversationAsync(string user1, string user2);
    }
}