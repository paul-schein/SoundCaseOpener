using Microsoft.Extensions.DependencyInjection;
using SoundCaseOpener.Core.Services;

namespace SoundCaseOpener.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<ILobbyService, LobbyService>();
        
        services.AddScoped<IUserService, UserService>();
        
        services.AddScoped<ISoundFileService, SoundFileService>();
        
        services.AddScoped<ISoundTemplateService, SoundTemplateService>();
        services.AddScoped<ICaseTemplateService, CaseTemplateService>();
        
        services.AddScoped<ISoundService, SoundService>();
        services.AddScoped<ICaseService, CaseService>();
    }
}
