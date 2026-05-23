using Aura.Application.Interfaces;
using Aura.Application.DTOs.Chat;
using Aura.Domain.Settings;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Aura.Application.Services;

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

    public async Task<string> GetChatResponseAsync(string question, string context, List<ChatMessageDTO>? history = null)
    {
        var url = "https://api.openai.com/v1/chat/completions";

        var systemPrompt = $@"Bạn là Chuyên gia tư vấn chiến lược hình ảnh cao cấp của Aura.
QUY TẮC ĐỊNH DẠNG (BẮT BUỘC):
1. TUYỆT ĐỐI KHÔNG dùng ký hiệu Markdown (không dùng **, không dùng #, không dùng [ ]).
2. TRÌNH BÀY: Dùng 2 lần xuống dòng (double newline) giữa các ý để tạo khoảng cách thoáng. Dùng dấu gạch ngang (-) đơn giản.
3. PHONG CÀCH: Chuyên nghiệp, súc tích và linh hoạt về độ dài. Khi khách hàng hỏi giới thiệu hoặc gợi ý sản phẩm, hãy trả lời ngắn gọn, cô đọng nhưng vẫn truyền tải đầy đủ ý nghĩa và nội dung chính của sản phẩm. Khi khách hàng yêu cầu phân tích, giải thích rõ hơn hoặc làm rõ chi tiết, hãy tự động trả lời chi tiết, đầy đủ và phân tích sâu sắc.
4. CHIẾN THUẬT: Chọn 1 gói sát nhất với ngân sách khách đưa ra (ưu tiên gói cao nhất trong tầm tiền) và giải thích ngắn gọn lý do.
5. QUY TẮC DỰ ÁN MẪU: Khi khách hỏi về ví dụ, mẫu hoặc muốn xem dự án đã làm, hãy liệt kê tên 2-3 dự án phù hợp nhất từ danh sách dự án thực tế được cung cấp bên dưới, nêu rõ Title và Category.
6. QUY TẮC QUYỀN LỢI: AI phải biết rằng các gói giá cao hơn luôn bao gồm toàn bộ quyền lợi của các gói thấp tiền hơn. Hãy dùng điều này để thuyết phục khách nâng cấp gói.
7. KẾT THÚC: 1 câu kêu gọi liên hệ hỗ trợ AURA để được tư vấn chuyên sâu hơn.

Kiến thức Aura và Dự án thực tế:
{context}";

        var messagesList = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (history != null)
        {
            foreach (var msg in history)
            {
                var apiRole = msg.Role.ToLower() == "bot" ? "assistant" : "user";
                messagesList.Add(new { role = apiRole, content = msg.Text });
            }
        }

        messagesList.Add(new { role = "user", content = question });

        var requestBody = new
        {
            model = _settings.Model,
            messages = messagesList.ToArray(),
            temperature = 0.7
        };

        var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Xin lỗi, tôi gặp sự cố khi xử lý câu hỏi.";
    }
}
