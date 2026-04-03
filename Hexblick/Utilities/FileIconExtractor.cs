using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace Hexblick.Utilities;

internal static class FileIconExtractor
{
    public static async Task<ImageSource> GetFileIconAsync(string path)
    {
        var storageFile = await StorageFile.GetFileFromPathAsync(path);

        using var thumbnail = await storageFile.GetScaledImageAsThumbnailAsync(
            ThumbnailMode.SingleItem,
            16,
            ThumbnailOptions.None);

        var bitmap = new BitmapImage();

        if (thumbnail.Type == ThumbnailType.Icon)
        {
            await bitmap.SetSourceAsync(thumbnail);
        }
        else
        {
            var imageBytes = await Task.Run(() =>
            {
                SHFILEINFOW sfi = default;

                var result = PInvoke.SHGetFileInfo(
                    path,
                    0,
                    ref sfi,
                    SHGFI_FLAGS.SHGFI_ICON |
                    SHGFI_FLAGS.SHGFI_SMALLICON |
                    SHGFI_FLAGS.SHGFI_ICONLOCATION |
                    SHGFI_FLAGS.SHGFI_SHELLICONSIZE |
                    SHGFI_FLAGS.SHGFI_SYSICONINDEX);

                try
                {
                    using var icon = Icon.FromHandle(sfi.hIcon);
                    using var ms = new MemoryStream();

                    icon.Save(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
                finally
                {
                    PInvoke.DestroyIcon(sfi.hIcon);
                }
            });

            using var ms = new MemoryStream(imageBytes);
            await bitmap.SetSourceAsync(ms.AsRandomAccessStream());
        }

        return bitmap;
    }
}
