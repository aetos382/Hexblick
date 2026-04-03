using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;

using Microsoft.UI.Xaml;

using R3;

namespace Hexblick.Windowing;

internal abstract class BaseWindow :
    Window,
    IDisposable
{
    private GCHandle<BaseWindow> _gcHandle;
    private readonly CompositeDisposable _disposables = [];

    protected BaseWindow(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.WindowServices = serviceProvider;

        var gcHandle = this._gcHandle = new GCHandle<BaseWindow>(this);

        unsafe
        {
            var result = PInvoke.SetWindowSubclass(
                new(this.NaiveHandle),
                &WindowProc,
                0,
                (nuint)gcHandle.ToIntPtr());
        }

        this._disposables.Add(gcHandle);
    }

    public IServiceProvider WindowServices { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._disposables.Dispose();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static LRESULT WindowProc(
        HWND windowHandle,
        uint message,
        WPARAM wParam,
        LPARAM lParam,
        nuint subclassId,
        nuint refData)
    {
        var gcHandle = GCHandle<BaseWindow>.FromIntPtr((nint)refData);

        switch (message)
        {
            case PInvoke.WM_NCDESTROY:
                gcHandle.Target.OnNcDestroy();
                break;
        }

        return PInvoke.DefSubclassProc(windowHandle, message, wParam, lParam);
    }

    private unsafe void OnNcDestroy()
    {
        var result = PInvoke.RemoveWindowSubclass((HWND)this.NaiveHandle, &WindowProc, 0);
        this._gcHandle.Dispose();
    }
}
