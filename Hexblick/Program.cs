using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using WinRT;

using Hexblick;
using Hexblick.Interactions;
using Hexblick.Presentations;
using Hexblick.Strings;
using Hexblick.Windowing;

ComWrappersSupport.InitializeComWrappers();

var appBuilder = Host.CreateApplicationBuilder(args);

var logging = appBuilder.Logging;

logging.ClearProviders();
logging.AddEventLog();
logging.AddEventSourceLogger();

if (Debugger.IsAttached)
{
    logging.AddDebug();
}

var services = appBuilder.Services;

services.UseWinApp<App>();
services.AddSingleton<IStringLoader, ResourceStringLoader>();
services.AddSingleton<IWindowManager, WindowManager>();

services.AddMessagePipe(static options =>
{
    options.EnableAutoRegistration = true;
});

services.AddScoped<IMultipleFileOpenPickerRequestHandler, MultipleFileOpenPickerRequestHandler>();
services.AddScoped<IConfirmSaveRequestHandler, ConfirmSaveRequestHandler>();
services.AddScoped<IChooseFontRequestHandler, ChooseFontRequestHandler>();
services.AddScoped<InteractionMessenger>();

services.AddScoped<MainWindow>();
services.AddScoped<MainWindowViewModel>();

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
