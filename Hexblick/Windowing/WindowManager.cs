using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using ZLinq;

namespace Hexblick.Windowing;

internal interface IWindowManager
{
    TWindow CreateWindow<TWindow>() where TWindow : Window;

    IReadOnlyCollection<Window> Windows { get; }

    bool TryGetWindowForElement(UIElement element, [MaybeNullWhen(false)] out Window window);
}

internal sealed class ScopedWindowManager :
    IWindowManager,
    IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    private sealed record WindowAssociatedData(
        Window Window,
        IServiceScope ServiceScope);

    private readonly ConcurrentDictionary<AppWindow, WindowAssociatedData> _data = new();

    public ScopedWindowManager(
        IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        this._scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public TWindow CreateWindow<TWindow>() where TWindow : Window
    {
        var scope = this._scopeFactory.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<TWindow>();

        var appWindow = window.AppWindow;

        this._data.TryAdd(appWindow, new(window, scope));

        appWindow.Destroying += this.OnAppWindowDestroying;

        return window;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Window> Windows
    {
        get
        {
            return this._data
                .AsValueEnumerable()
                .Select(static x => x.Value.Window)
                .ToArray();
        }
    }

    /// <inheritdoc />
    public bool TryGetWindowForElement(
        UIElement element,
        [MaybeNullWhen(false)] out Window window)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (element.XamlRoot is not { } xamlRoot)
        {
            window = null;
            return false;
        }

        foreach (var (_, (w, _)) in this._data)
        {
            if (w.Content.XamlRoot == xamlRoot)
            {
                window = w;
                return true;
            }
        }

        window = null;
        return false;
    }

    private void OnAppWindowDestroying(AppWindow sender, object args)
    {
        sender.Destroying -= this.OnAppWindowDestroying;

        if (this._data.Remove(sender, out var data))
        {
            data.ServiceScope.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var key in this._data.Keys.AsValueEnumerable().ToArray())
        {
            if (this._data.Remove(key, out var data))
            {
                data.ServiceScope.Dispose();
            }
        }
    }
}
