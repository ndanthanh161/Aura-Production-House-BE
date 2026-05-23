namespace Aura.Application.DTOs.Chat
{
    public class ChatMessageDTO
    {
        public string Role { get; set; } = string.Empty; // "user" hoặc "bot"
        public string Text { get; set; } = string.Empty; // Nội dung tin nhắn
    }
}
