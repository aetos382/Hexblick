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

internal sealed partial class WindowManager :
    IWindowManager
{
    private readonly IServiceProvider _serviceProvider;

    private sealed partial record WindowContext(
        Window Window,
        IServiceScope Scope) : IDisposable
    {
        public void Dispose()
        {
            this.Scope.Dispose();
        }
    }

    private readonly ConditionalWeakTable<AppWindow, WindowContext> _windows = [];

    public WindowManager(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TWindow CreateWindow<TWindow>() where TWindow : Window
    {
        var scope = this._serviceProvider.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<TWindow>();

        this._windows.Add(window.AppWindow, new(window, scope));

        window.AppWindow.Destroying += this.OnAppWindowDestroying;

        return window;
    }

    private void OnAppWindowDestroying(AppWindow sender, object args)
    {
        sender.Destroying -= this.OnAppWindowDestroying;

        if (this._windows.Remove(sender, out var context))
        {
            context.Dispose();
        }
    }

    /// <inheritdoc />
    public IEnumerable<Window> Windows
    {
        get
        {
            var windows = this._windows;

            foreach (var (_, (window, _)) in windows)
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
