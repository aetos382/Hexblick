using System.Runtime.Versioning;

using Windows.Win32.Foundation;

namespace Windows.Win32;

static partial class PInvoke
{
    [SupportedOSPlatform("windows5.0")]
    internal static unsafe BOOL SetProp(HWND hWnd, string propName, nuint data)
    {
        fixed (char* pString = propName)
        {
            return SetProp(hWnd, pString, (HANDLE)data);
        }
    }
}
