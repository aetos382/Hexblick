using System.Runtime.InteropServices;

using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

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
        using var app = appBuilder.Build();

        await app.StartAsync().ConfigureAwait(false);

        Application.Start(p =>
        {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);

            _ = new App();
        });

        await app.StopAsync().ConfigureAwait(false);
    }

    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();
}
