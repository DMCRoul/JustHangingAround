namespace JustHangingAround.Shared.Models
{
    public class ChatMessage
    {
        public string Username { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}