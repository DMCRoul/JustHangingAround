namespace JustHangingAround.Client.ViewModels
{
    public class ChatMessageViewModel
    {
        public string Username { get; set; } = "";
        public string Text { get; set; } = "";
        public bool IsOwnMessage { get; set; }
    }
}