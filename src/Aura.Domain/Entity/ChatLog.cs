namespace Aura.Domain.Entity
{
    public class ChatLog
    {
        public Guid Id { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public string? SessionId { get; set; } // Để nhóm các tin nhắn trong 1 phiên chat
        public bool IsPinned { get; set; } = false; // Đánh dấu quan trọng
        public DateTime CreatedAt { get; set; }
    }
}
