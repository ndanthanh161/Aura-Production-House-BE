using Aura.Application.Interfaces;
using Aura.Domain.Settings;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Aura.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly AiSettings _settings;
        private readonly HttpClient _httpClient;

        public AiService(IOptions<AiSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var url = "https://api.openai.com/v1/embeddings";

            var requestBody = new
            {
                model = _settings.EmbeddingModel,
                input = text
            };

            var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var values = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");

            var result = new float[values.GetArrayLength()];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = values[i].GetSingle();
            }

            return result;
        }

        public async Task<string> GetChatResponseAsync(string question, string context)
        {
            var url = "https://api.openai.com/v1/chat/completions";

            var systemPrompt = $@"Bạn là Chuyên gia tư vấn chiến lược hình ảnh cao cấp của Aura.
QUY TẮC ĐỊNH DẠNG (BẮT BUỘC):
1. TUYỆT ĐỐI KHÔNG dùng ký hiệu Markdown (không dùng **, không dùng #, không dùng [ ]).
2. TRÌNH BÀY: Dùng 2 lần xuống dòng (double newline) giữa các ý để tạo khoảng cách thoáng. Dùng dấu gạch ngang (-) đơn giản.
3. PHONG CÁCH: Trả lời cực kỳ ngắn gọn (dưới 100 từ), chuyên nghiệp và súc tích.
4. CHIẾN THUẬT: Chọn 1 gói sát nhất với ngân sách khách đưa ra (ưu tiên gói cao nhất trong tầm tiền) và giải thích ngắn gọn lý do.
5. QUY TẮC QUYỀN LỢI: AI phải biết rằng các gói giá cao hơn luôn bao gồm toàn bộ quyền lợi của các gói thấp tiền hơn. Hãy dùng điều này để thuyết phục khách nâng cấp gói.
6. KẾT THÚC: 1 câu kêu gọi liên hệ hỗ trợ AURA để được tư vấn chuyên sâu hơn.

Kiến thức Aura:
{context}";

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = question }
                },
                temperature = 0.7
            };

            var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Xin lỗi, tôi gặp sự cố khi xử lý câu hỏi.";
        }
    }
}
