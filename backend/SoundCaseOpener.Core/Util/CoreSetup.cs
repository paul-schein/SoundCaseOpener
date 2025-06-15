using Microsoft.Extensions.DependencyInjection;
using SoundCaseOpener.Core.Services;

namespace SoundCaseOpener.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISoundFileService, SoundFileService>();
        services.AddScoped<ISoundTemplateService, SoundTemplateService>();
    }
}
