using Microsoft.Extensions.DependencyInjection;

namespace SoundCaseOpener.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
    }
}
