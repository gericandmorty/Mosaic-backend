using backend.Modules.Music.Services;

namespace backend.Modules.Music
{
    public static class MusicModuleExtensions
    {
        public static IServiceCollection AddMusicModule(this IServiceCollection services)
        {
            services.AddScoped<IMusicService, YoutubeMusicService>();
            services.AddScoped<ILibraryService, LibraryService>();
            return services;
        }
    }
}
