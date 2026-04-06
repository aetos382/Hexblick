using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;

using Windows.Win32;
using Windows.Win32.Foundation;

using WinRT.Interop;

namespace Hexblick.Windowing;

internal static class WindowExtensions
{
    extension(Window window)
    {
        public nint NaiveHandle => WindowNative.GetWindowHandle(window);

        public IServiceProvider WindowServices
        {
            get
            {
                ArgumentNullException.ThrowIfNull(window);
                return window.GetRequiredContext().ServiceScope.ServiceProvider;
            }
        }

        public void TrackLifetime(IDisposable disposable)
        {
            ArgumentNullException.ThrowIfNull(window);
            ArgumentNullException.ThrowIfNull(disposable);

            var disposables = window.GetRequiredContext().Disposables;
            disposables.Add(disposable);
        }

        private bool TryGetContext([MaybeNullWhen(false)] out WindowContext context)
        {
            // https://github.com/microsoft/CsWin32/issues/1674
            var prop = PInvoke.GetProp((HWND)window.NaiveHandle, WindowProps.ServiceContext).DangerousGetHandle();
            if (prop == 0)
            {
                context = null;
                return false;
            }

            context = GCHandle<WindowContext>.FromIntPtr(prop).Target;
            return true;
        }

        private WindowContext GetRequiredContext()
        {
            if (!window.TryGetContext(out var context))
            {
                throw new InvalidOperationException();
            }

            return context;
        }
    }
}
