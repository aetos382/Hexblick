using Hexblick;
using Hexblick.Localization;
using Hexblick.Services;
using Hexblick.UI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WinRT;

ComWrappersSupport.InitializeComWrappers();

var appBuilder = Host.CreateApplicationBuilder(args);

var services = appBuilder.Services;

services.UseWinApp<App>();
services.AddSingleton<IEditorControlViewModelFactory, EditorControlViewModelFactory>();

services.AddTransient<MainWindow>();
services.AddTransient<MainWindowViewModel>();
services.AddTransient<EditorControlViewModel>();

services.AddSingleton<IStringLoader, ResourceStringLoader>();
services.AddSingleton<IDialogService, DialogService>();

services.AddMessagePipe(static options =>
{
    options.EnableAutoRegistration = true;
});

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
