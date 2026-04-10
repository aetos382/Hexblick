using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WinRT;

using Hexblick;
using Hexblick.Interactions;
using Hexblick.Localization;
using Hexblick.Presentations;
using Hexblick.Windowing;

ComWrappersSupport.InitializeComWrappers();

var appBuilder = Host.CreateApplicationBuilder(args);

var services = appBuilder.Services;

services.UseWinApp<App>();
services.AddSingleton<IStringLoader, ResourceStringLoader>();
services.AddSingleton<IWindowManager, WindowManager>();

services.AddMessagePipe(static options =>
{
    options.EnableAutoRegistration = true;
});

services.AddScoped<MainWindow>();
services.AddScoped<MainWindowViewModel>();

services.AddScoped<InteractionMessenger>();
services.AddScoped<IMultipleFileOpenPickerRequestHandler, MultipleFileOpenPickerRequestHandler>();
services.AddScoped<IConfirmSaveRequestHandler, ConfirmSaveRequestHandler>();

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
