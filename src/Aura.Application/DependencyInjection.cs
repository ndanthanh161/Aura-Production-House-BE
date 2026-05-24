using Aura.Application.Interfaces;
using Aura.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aura.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ===== Business Services =====
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IPhotographerService, PhotographerService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();
        
        // ===== AI Services =====
        services.AddHttpClient<IAiService, AiService>();
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
