using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using Windows.Win32;
using Windows.Win32.Foundation;

using R3;

namespace Hexblick.Windowing;

internal abstract class BaseWindow :
    Window,
    IServceableWindow
{
    private readonly CompositeDisposable _disposables = [];

    protected BaseWindow()
    {
        var gcHandle = new GCHandle<BaseWindow>(this);

        unsafe
        {
            var result = PInvoke.SetWindowSubclass(
                (HWND)this.NaiveHandle,
                &WindowProc,
                0,
                (nuint)gcHandle.ToIntPtr());
        }

        this._disposables.Add(gcHandle);
        this._disposables.Add(this._scopeHolder);
    }

    private readonly SingleAssignmentDisposable _scopeHolder = new();

    void IServceableWindow.SetServiceScope(IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        this._scopeHolder.Disposable = scope;
        this.WindowServices = scope.ServiceProvider;
    }

    public T TrackLifetime<T>(T item) where T : IDisposable
    {
        ArgumentNullException.ThrowIfNull(item);

        this._disposables.Add(item);
        return item;
    }

    public IServiceProvider WindowServices
    {
        get
        {
            if (field is not { } sp)
            {
                throw new InvalidOperationException();
            }

            return sp;
        }

        private set;
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

            default:
                break;
        }

        return PInvoke.DefSubclassProc(windowHandle, message, wParam, lParam);
    }

    private unsafe void OnNcDestroy()
    {
        var result = PInvoke.RemoveWindowSubclass((HWND)this.NaiveHandle, &WindowProc, 0);
        this._disposables.Dispose();
    }
}
