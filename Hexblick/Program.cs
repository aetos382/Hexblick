using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using WinRT;

namespace Hexblick;

internal static partial class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        XamlCheckProcessRequirements();
        ComWrappersSupport.InitializeComWrappers();

        var appBuilder = Host.CreateApplicationBuilder(args);

        var services = appBuilder.Services;

        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<App>();
        services.AddHostedService<ApplicationService>();
        services.Replace(ServiceDescriptor.Singleton<IHostLifetime, WinUIApplicationLifetime>());

        using var host = appBuilder.Build();

        await host.StartAsync().ConfigureAwait(false);

        var appThread = host.Services.GetRequiredService<IApplicationThread>();
        _ = appThread.RunAsync();

        await host.WaitForShutdownAsync().ConfigureAwait(false);
    }

    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();
}
