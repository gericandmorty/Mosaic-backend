using backend.Modules.Auth.Interfaces;
using backend.Modules.Auth.Services;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
