using backend.Modules.History.Services;

namespace backend.Modules.History;

public static class HistoryModuleExtensions
{
    public static IServiceCollection AddHistoryModule(this IServiceCollection services)
    {
        services.AddScoped<IHistoryService, HistoryService>();
        return services;
    }
}
