using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
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

    private readonly ConditionalWeakTable<AppWindow, Window> _windows = new();

    public ScopedWindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow CreateWindow<TWindow>() where TWindow : Window
    {
        var window = this._serviceProvider.GetRequiredService<TWindow>();

        this._windows.Add(window.AppWindow, window);

        return window;
    }

    /// <inheritdoc />
    public IEnumerable<Window> Windows
    {
        get
        {
            var windows = this._windows;

            foreach (var (_, window) in windows)
            {
                yield return window;
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
