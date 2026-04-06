using System.Runtime.InteropServices;
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

    [LibraryImport("user32.dll", EntryPoint = "GetPropW")]
    internal static extern nuint GetProp(HWND hWnd, PCWSTR propName);

    [LibraryImport("user32.dll", EntryPoint = "SetPropW")]
    internal static extern BOOL SetProp(HWND hWnd, PCWSTR propName, nuint data);

    [LibraryImport("user32.dll", EntryPoint = "RemovePropW")]
    internal static extern nuint RemoveProp(HWND hWnd, PCWSTR propName);
}
