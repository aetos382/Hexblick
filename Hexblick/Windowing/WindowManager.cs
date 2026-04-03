using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Hexblick.Windowing;

internal interface IWindowManager
{
    TWindow CreateWindow<TWindow>() where TWindow : Window, IServiceableWindow;

    IEnumerable<Window> Windows { get; }

    bool TryGetServiceScope(
        Window window,
        [MaybeNullWhen(false)] out IServiceScope scope);

    bool TryGetWindowForElement(
        UIElement element,
        [MaybeNullWhen(false)] out Window window);
}

internal sealed class ScopedWindowManager :
    IWindowManager
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConditionalWeakTable<Window, IServiceScope> _windows = new();

    public ScopedWindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow CreateWindow<TWindow>() where TWindow : Window, IServiceableWindow
    {
        var sp = this._serviceProvider;
        var scope = sp.CreateScope();

        var window = ActivatorUtilities.CreateInstance<TWindow>(scope.ServiceProvider);
        window.SetServiceScope(scope);

        this._windows.TryAdd(window, scope);
        return window;
    }

    /// <inheritdoc />
    public IEnumerable<Window> Windows
    {
        get
        {
            var windows = this._windows;

            foreach (var (w, _) in windows)
            {
                yield return w;
            }
        }
    }

    public bool TryGetServiceScope(
        Window window,
        [MaybeNullWhen(false)] out IServiceScope scope)
    {
        return this._windows.TryGetValue(window, out scope);
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
