using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Hexblick.Windowing;

internal interface IWindowManager
{
    TWindow CreateWindow<TWindow>() where TWindow : Window;

    IEnumerable<Window> Windows { get; }

    bool TryGetWindowForElement(
        UIElement element,
        [MaybeNullWhen(false)] out Window window);
}

internal sealed class ScopedWindowManager :
    IWindowManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentBag<WeakReference<Window>> _windows = new();

    public ScopedWindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow CreateWindow<TWindow>() where TWindow : Window
    {
        var sp = this._serviceProvider;
        var scope = sp.CreateScope();

        var window = ActivatorUtilities.CreateInstance<TWindow>(scope.ServiceProvider, scope);
        if (window is IServceableWindow sw)
        {
            sw.SetServiceScope(scope);
        }

        this._windows.Add(new WeakReference<Window>(window));
        return window;
    }

    /// <inheritdoc />
    public IEnumerable<Window> Windows
    {
        get
        {
            foreach (var r in this._windows)
            {
                if (r.TryGetTarget(out var w))
                {
                    yield return w;
                }
            }
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

        foreach (var w in this.Windows)
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
}
