using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace Hexblick.Windowing;

internal static class WindowExtensions
{
    extension(Window window)
    {
        public nint NaiveHandle => WindowNative.GetWindowHandle(window);
    }
}
