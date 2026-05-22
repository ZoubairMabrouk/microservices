using ConversationHistoryService.Data;
using ConversationHistoryService.Repositories;
using ConversationHistoryService.Services;
using Microsoft.EntityFrameworkCore;

namespace ConversationHistoryService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddConversationHistoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConversationDbContext>(options =>
            options.UseInMemoryDatabase("ConversationDb"));

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationService, ConversationHistoryService.Services.ConversationService>();

        return services;
    }
}
