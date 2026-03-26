using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Hexblick;

public interface IWindowManager
{
    TWindow Create<TWindow>() where TWindow : Window;

    IEnumerable<Window> GetWindows();
}

internal sealed class WindowManager :
    IWindowManager,
    IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<AppWindow, Window> _windows = new();
    private readonly ConditionalWeakTable<AppWindow, IServiceScope> _windowScopedServiceProviders = new();

    public WindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow Create<TWindow>() where TWindow : Window
    {
        var serviceProvider = this._serviceProvider;
        var window = serviceProvider.GetRequiredService<TWindow>();

        var appWindow = window.AppWindow;

        this._windows.GetOrAdd(appWindow, window);

        window.AppWindow.Closing += OnWindowClosing;

        this._windowScopedServiceProviders.Add(appWindow, serviceProvider.CreateScope());

        return window;

        void OnWindowClosing(AppWindow sender, object args)
        {
            sender.Closing -= OnWindowClosing;

            this._windows.Remove(sender, out _);

            if (this._windowScopedServiceProviders.Remove(sender, out var scope))
            {
                scope.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<Window> GetWindows()
    {
        return this._windows.Values;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var (_, value) in this._windowScopedServiceProviders)
        {
            value.Dispose();
        }
    }
}

internal static class WindowExtensions
{
    extension(Window window)
    {
        IServiceProvider Services
        {
            get
            {
            }
        }
    }
}
