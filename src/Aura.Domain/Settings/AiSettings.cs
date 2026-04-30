namespace Aura.Domain.Settings
{
    public class AiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini"; // Hoặc "gpt-4o"
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    }
}
