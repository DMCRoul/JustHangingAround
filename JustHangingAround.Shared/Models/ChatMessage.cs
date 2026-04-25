namespace JustHangingAround.Shared.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Recipient { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // --- Вложения ---

        public string? AttachmentUrl { get; set; }

        public string? AttachmentFileName { get; set; }

        public string? AttachmentContentType { get; set; }

        public long? AttachmentSize { get; set; }
    }
}