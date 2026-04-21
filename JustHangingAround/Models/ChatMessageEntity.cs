namespace JustHangingAround.Models
{
    public class ChatMessageEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}