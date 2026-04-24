public class ChatMessage
{
    public string Username { get; set; } = string.Empty; // кто отправил
    public string Recipient { get; set; } = string.Empty; // кому
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

}