using System;

using Microsoft.UI.Xaml;

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
                if (window is not BaseWindow b)
                {
                    throw new NotSupportedException();
                }

                return b.WindowServices;
            }
        }
    }
}
