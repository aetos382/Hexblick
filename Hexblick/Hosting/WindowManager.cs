using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Hexblick.Hosting;

// public な App が依存しているので internal にできない
#pragma warning disable CA1515

public interface IWindowManager
{
    TWindow Create<TWindow>() where TWindow : Window;

    IEnumerable<Window> GetWindows();
}

#pragma warning restore

#pragma warning disable CA1812

internal sealed class WindowManager :
    IWindowManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<AppWindow, Window> _windows = new();

    public WindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow Create<TWindow>() where TWindow : Window
    {
        var scope = this._serviceProvider.CreateScope();

        var window = scope.ServiceProvider.GetRequiredService<TWindow>();

        var appWindow = window.AppWindow;

        this._windows.GetOrAdd(appWindow, window);

        window.AppWindow.Destroying += OnWindowDestroying;

        WindowScopeStore.Instance.SetWindowScope(window, scope);

        return window;

        void OnWindowDestroying(AppWindow sender, object args)
        {
            sender.Destroying -= OnWindowDestroying;

            this._windows.Remove(sender, out _);
        }
    }

    /// <inheritdoc />
    public IEnumerable<Window> GetWindows()
    {
        return this._windows.Values;
    }
}

#pragma warning restore

file class WindowScopeStore
{
    private readonly ConditionalWeakTable<AppWindow, IServiceScope> _scopes = [];

    public void SetWindowScope(Window window, IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(scope);

        var appWindow = window.AppWindow;

        this._scopes.GetOrAdd(appWindow, scope);

        appWindow.Destroying += OnWindowDestroying;

        void OnWindowDestroying(AppWindow sender, object args)
        {
            sender.Destroying -= OnWindowDestroying;

            if (this._scopes.Remove(sender, out var scope))
            {
                scope.Dispose();
            }
        }
    }

    public bool TryGetWindowScope(
        Window window,
        [MaybeNullWhen(false)] out IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(window);

        return this._scopes.TryGetValue(window.AppWindow, out scope);
    }

    private WindowScopeStore()
    {
    }

    ~WindowScopeStore()
    {
        foreach (var (_, scope) in this._scopes)
        {
            scope.Dispose();
        }
    }

    public static readonly WindowScopeStore Instance = new();
}

internal static class WindowExtensions
{
    extension(Window window)
    {
        public IServiceProvider Services
        {
            get
            {
                ArgumentNullException.ThrowIfNull(window);

                if (!WindowScopeStore.Instance.TryGetWindowScope(window, out var scope))
                {
                    throw new InvalidOperationException();
                }

                return scope.ServiceProvider;
            }
        }
    }
}
