namespace System.Runtime.InteropServices;

internal static class GCHandleExtensions
{
    extension<T>(GCHandle<T> gcHandle) where T : class?
    {
        public nint ToIntPtr() => GCHandle<T>.ToIntPtr(gcHandle);
    }

    extension(GCHandle gcHandle)
    {
        public nint ToIntPtr() => GCHandle.ToIntPtr(gcHandle);
    }
}
