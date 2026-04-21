using Microsoft.Extensions.DependencyInjection;

namespace Aura.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Sau này đăng ký Services, AutoMapper, MediatR, FluentValidation ở đây
            return services;
        }
    }
}
