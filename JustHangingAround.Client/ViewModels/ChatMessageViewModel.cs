using System;

namespace JustHangingAround.Client.ViewModels
{
    public class ChatMessageViewModel
    {
        public string Username { get; set; } = "";

        public string Text { get; set; } = "";

        public bool IsOwnMessage { get; set; }

        // --- Вложения ---

        public string? AttachmentUrl { get; set; }

        public string? AttachmentFileName { get; set; }

        public string? AttachmentContentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Time =>
    CreatedAt.ToLocalTime().ToString("HH:mm");

        public string Date =>
            CreatedAt.ToLocalTime().ToString("dd.MM");


        public bool HasAttachment => !string.IsNullOrWhiteSpace(AttachmentUrl);

        public bool IsImage =>
            (!string.IsNullOrWhiteSpace(AttachmentContentType) &&
             AttachmentContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(AttachmentFileName) &&
             (
                 AttachmentFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                 AttachmentFileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                 AttachmentFileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                 AttachmentFileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                 AttachmentFileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                 AttachmentFileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
             ));
    }
}