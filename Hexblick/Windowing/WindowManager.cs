using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.Win32;
using Windows.Win32.Foundation;

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
        var sp = this._serviceProvider;
        var scope = sp.CreateScope();

        var window = scope.ServiceProvider.GetRequiredService<TWindow>();

        var windowContext = new WindowContext(window, scope);
        var gcHandle = GCHandle.Alloc(windowContext);

        PInvoke.SetProp((HWND)window.NaiveHandle, WindowProps.ServiceContext, (nuint)GCHandle.ToIntPtr(gcHandle));

        this._windows.Add(window.AppWindow, window);

        window.AppWindow.Destroying += this.OnAppWindowDestroying;

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

    private void OnAppWindowDestroying(AppWindow sender, object args)
    {
        sender.Destroying -= this.OnAppWindowDestroying;

        if (this._windows.Remove(sender, out var window))
        {
            var prop = PInvoke.RemoveProp((HWND)window.NaiveHandle, WindowProps.ServiceContext);

            var gcHandle = GCHandle<WindowContext>.FromIntPtr((nint)prop);
            gcHandle.Target.Dispose();
            gcHandle.Dispose();
        }
    }
}
