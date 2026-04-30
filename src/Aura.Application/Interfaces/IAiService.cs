namespace Aura.Application.Interfaces
{
    public interface IAiService
    {
        Task<float[]> GetEmbeddingAsync(string text);
        Task<string> GetChatResponseAsync(string question, string context);
    }
}
