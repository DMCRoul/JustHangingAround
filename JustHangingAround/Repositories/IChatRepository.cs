using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public interface IChatRepository
    {
        void Add(ChatMessage message);
        List<ChatMessage> GetAll();
    }
}