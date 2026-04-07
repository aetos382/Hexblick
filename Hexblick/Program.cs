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
services.AddSingleton<IWindowManager, ScopedWindowManager>();

services.AddMessagePipe(static options =>
{
    options.EnableAutoRegistration = true;
});

services.AddScoped<MainWindow>();
services.AddScoped<MainWindowViewModel>();
services.AddScoped<ServiceScopeMarker>();
services.AddScoped<InteractionMessenger>();
services.AddScoped<EditorControlViewModel>();
services.AddScoped<IDocumentManager, DocumentManager>();

services.AddScoped<IMultipleFileOpenPickerRequestHandler, MultipleFileOpenPickerRequestHandler>();
services.AddScoped<IConfirmSaveRequesetHandler, ConfirmSaveRequestHandler>();

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
