using Hexblick;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WinRT;

ComWrappersSupport.InitializeComWrappers();

var appBuilder = Host.CreateApplicationBuilder(args);

var services = appBuilder.Services;
services.UseWinApp<App>();
services.AddSingleton<IWindowManager, WindowManager>();
services.AddTransient<MainWindow>();

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
