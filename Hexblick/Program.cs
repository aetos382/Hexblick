using Hexblick;
using Hexblick.Hosting;
using Hexblick.Localization;
using Hexblick.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WinRT;

using MainWindowViewModel = Hexblick.ViewModels.MainWindowViewModel;
using EditorControlViewModel = Hexblick.ViewModels.EditorControlViewModel;

ComWrappersSupport.InitializeComWrappers();

var appBuilder = Host.CreateApplicationBuilder(args);

var services = appBuilder.Services;

services.UseWinApp<App>();
services.AddSingleton<IWindowManager, WindowManager>();
services.AddSingleton<IEditorControlViewModelFactory, EditorControlViewModelFactory>();

services.AddTransient<MainWindow>();
services.AddTransient<MainWindowViewModel>();
services.AddTransient<EditorControlViewModel>();

services.AddSingleton<IStringLoader, ResourceStringLoader>();
services.AddSingleton<IDialogService, DialogService>();

using var host = appBuilder.Build();

await host.RunAsync().ConfigureAwait(false);
