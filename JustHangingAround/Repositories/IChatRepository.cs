using JustHangingAround.Shared.Models;

namespace JustHangingAround.Repositories
{
    public interface IChatRepository
    {
        List<ChatMessage> GetAll();
        ChatMessage Add(string username, string text);
    }
}