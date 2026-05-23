using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Application.DTOs.Chat;
using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;
using Pgvector;

namespace Aura.Application.Services;

public class ChatService : IChatService
{
    private readonly IAiService _aiService;
    private readonly IKnowledgeRepository _knowledgeRepo;
    private readonly IChatLogRepository _chatLogRepo;
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IPackageRepository _packageRepo;

    public ChatService(
        IAiService aiService, 
        IKnowledgeRepository knowledgeRepo, 
        IChatLogRepository chatLogRepo,
        IPortfolioRepository portfolioRepo,
        IPackageRepository packageRepo)
    {
        _aiService = aiService;
        _knowledgeRepo = knowledgeRepo;
        _chatLogRepo = chatLogRepo;
        _portfolioRepo = portfolioRepo;
        _packageRepo = packageRepo;
    }

    public async Task<string> ProcessMessageAsync(string message, List<ChatMessageDTO>? history = null)
    {
        var msgLower = message.ToLower();

        // 1. Get embedding for the user message
        var embedding = await _aiService.GetEmbeddingAsync(message);
        var vector = new Vector(embedding);

        // 2. Search for relevant context in Knowledge Base using Repository (ALWAYS PRIORITIZED)
        var relevantContext = await _knowledgeRepo.SearchRelevantContentAsync(vector, 8);
        var contextParts = new List<string>();

        if (relevantContext != null && relevantContext.Any())
        {
            contextParts.Add("KIẾN THỨC NỀN TẢNG AURA (ƯU TIÊN HÀNG ĐẦU):\n" + string.Join("\n---\n", relevantContext));
        }

        // 3. Dynamic RAG for Packages (Pricing / Services / Benefits)
        bool asksAboutPackages = msgLower.Contains("gói") || 
                                 msgLower.Contains("giá") || 
                                 msgLower.Contains("bao nhiêu") || 
                                 msgLower.Contains("chi phí") || 
                                 msgLower.Contains("combo") || 
                                 msgLower.Contains("package") || 
                                 msgLower.Contains("báo giá") || 
                                 msgLower.Contains("dịch vụ") || 
                                 msgLower.Contains("tiền") || 
                                 msgLower.Contains("ngân sách");

        if (asksAboutPackages)
        {
            var packages = await _packageRepo.GetAllAsync(onlyActive: true);
            if (packages != null && packages.Any())
            {
                var packageStrings = packages.Select(p => 
                    $"- Gói dịch vụ: {p.Name}\n" +
                    $"  Giá: {p.Price:N0} VND\n" +
                    $"  Mô tả: {p.Description ?? "Chưa có mô tả"}\n" +
                    $"  Quyền lợi:\n" + string.Join("\n", p.Benefits.Select(b => $"    + {b}"))
                );
                contextParts.Add("DANH SÁCH CÁC GÓI DỊCH VỤ CỦA AURA (HÃY BÁO GIÁ VÀ TƯ VẤN CHO KHÁCH HÀNG DỰA TRÊN THÔNG TIN NÀY):\n" + string.Join("\n\n", packageStrings));
            }
        }

        // 4. Dynamic RAG for Portfolio Items (Previous works / Samples)
        bool asksAboutPortfolios = msgLower.Contains("dự án") || 
                                   msgLower.Contains("mẫu") || 
                                   msgLower.Contains("thực tế") || 
                                   msgLower.Contains("đã làm") || 
                                   msgLower.Contains("xem") || 
                                   msgLower.Contains("portfolio") || 
                                   msgLower.Contains("sản phẩm") || 
                                   msgLower.Contains("chụp") || 
                                   msgLower.Contains("quay") || 
                                   msgLower.Contains("video") || 
                                   msgLower.Contains("quảng cáo") || 
                                   msgLower.Contains("hình ảnh");

        if (asksAboutPortfolios)
        {
            var portfolios = await _portfolioRepo.GetPublishedAsync();
            if (portfolios != null && portfolios.Any())
            {
                // Intelligent semantic routing: Filter by Category if user asks about a specific one
                if (msgLower.Contains("chụp") || msgLower.Contains("ảnh") || msgLower.Contains("photography"))
                {
                    portfolios = portfolios.Where(p => p.Category == PortfolioCategory.Photography);
                }
                else if (msgLower.Contains("phim") || msgLower.Contains("video") || msgLower.Contains("videography"))
                {
                    portfolios = portfolios.Where(p => p.Category == PortfolioCategory.Videography);
                }
                else if (msgLower.Contains("cá nhân") || msgLower.Contains("branding") || msgLower.Contains("thương hiệu"))
                {
                    portfolios = portfolios.Where(p => p.Category == PortfolioCategory.PersonalBranding);
                }
                else if (msgLower.Contains("quảng cáo") || msgLower.Contains("thương mại") || msgLower.Contains("commercial"))
                {
                    portfolios = portfolios.Where(p => p.Category == PortfolioCategory.Commercial);
                }
                else if (msgLower.Contains("mạng xã hội") || msgLower.Contains("social") || msgLower.Contains("content"))
                {
                    portfolios = portfolios.Where(p => p.Category == PortfolioCategory.SocialContent);
                }

                var selectedPortfolios = portfolios.Take(8).ToList();
                if (selectedPortfolios.Any())
                {
                    var portfolioStrings = selectedPortfolios.Select(p => 
                        $"- Dự án mẫu: {p.Title}\n" +
                        $"  Hạng mục: {GetCategoryVietnamese(p.Category)}\n" +
                        $"  Khách hàng: {p.ClientName ?? "Khách hàng của Aura"}\n" +
                        $"  Mô tả chi tiết: {p.Content ?? "Chưa có mô tả chi tiết"}"
                    );
                    contextParts.Add("DANH SÁCH CÁC DỰ ÁN THỰC TẾ ĐÃ THỰC HIỆN PHÙ HỢP (DÙNG ĐỂ GỢI Ý MẪU KHI KHÁCH HÀNG HỎI): \n" + string.Join("\n\n", portfolioStrings));
                }
            }
        }

        var contextString = string.Join("\n\n====================\n\n", contextParts);

        // 5. Get response from AI
        var botResponse = await _aiService.GetChatResponseAsync(message, contextString, history);

        // 4. Save log to DB using Repository
        var log = ChatMapper.ToLogEntity(message, botResponse);
        await _chatLogRepo.AddAsync(log);

        return botResponse;
    }

    public async Task IngestKnowledgeAsync(string content, string category)
    {
        var embedding = await _aiService.GetEmbeddingAsync(content);
        var knowledge = ChatMapper.ToKnowledgeEntity(content, category, embedding);

        await _knowledgeRepo.AddAsync(knowledge);
    }

    public async Task<IEnumerable<AuraKnowledge>> GetKnowledgeBaseAsync()
    {
        return await _knowledgeRepo.GetAllAsync();

    }

    public async Task DeleteKnowledgeAsync(Guid id)
    {
        var knowledge = await _knowledgeRepo.GetByIdAsync(id);
        if (knowledge != null)
        {
            await _knowledgeRepo.DeleteAsync(knowledge);
        }
    }


    public async Task<IEnumerable<ChatLog>> GetChatLogsAsync()
    {
        return await _chatLogRepo.GetLatestLogsAsync(100);
    }


    public async Task ToggleChatLogPinAsync(Guid id)
    {
        var log = await _chatLogRepo.GetByIdAsync(id);
        if (log != null)
        {
            log.IsPinned = !log.IsPinned;
            await _chatLogRepo.UpdateAsync(log);
        }
    }

    private string GetCategoryVietnamese(PortfolioCategory category)
    {
        return category switch
        {
            PortfolioCategory.Photography => "Chụp ảnh / Nhiếp ảnh",
            PortfolioCategory.Videography => "Quay phim / Sản xuất video",
            PortfolioCategory.PersonalBranding => "Xây dựng thương hiệu cá nhân",
            PortfolioCategory.Commercial => "Quảng cáo / Thương mại",
            PortfolioCategory.SocialContent => "Sản xuất nội dung mạng xã hội",
            _ => category.ToString()
        };
    }
}
