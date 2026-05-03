using backend.Modules.Users.Services;
using backend.Infrastructure.Cloudinary;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Users;

public static class UserModuleExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddSingleton<CloudinaryService>();
        services.AddScoped<UserService>();
        return services;
    }
}
