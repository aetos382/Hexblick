using System.Runtime.InteropServices;

using Windows.Win32.Foundation;

namespace Windows.Win32;

internal static unsafe partial class PInvoke
{
    // CsWin32 を使うと prop の型に SafeFileHandle とかを当てるのが気に入らないから自前宣言
    [LibraryImport("user32.dll", EntryPoint = "GetPropW", StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial nuint GetProp(HWND hWnd, string propName);

    [LibraryImport("user32.dll", EntryPoint = "SetPropW", StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial BOOL SetProp(HWND hWnd, string propName, nuint data);

    [LibraryImport("user32.dll", EntryPoint = "RemovePropW", StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial nuint RemoveProp(HWND hWnd, string propName);
}
