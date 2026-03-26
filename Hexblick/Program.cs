using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using WinRT;

namespace Hexblick;

internal static partial class Program
{
    public static async Task Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();

        var appBuilder = Host.CreateApplicationBuilder(args);

        var services = appBuilder.Services;

        services.AddSingleton<App>();
        services.Replace(ServiceDescriptor.Singleton<IHostLifetime, WinUIApplicationLifetime<App>>());

        using var host = appBuilder.Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
