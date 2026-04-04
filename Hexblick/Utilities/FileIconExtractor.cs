using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Hexblick.Utilities;

internal static class FileIconExtractor
{
    public static async ValueTask<ImageSource?> GetFileIconAsync(string path)
    {
        using var icon = Icon.ExtractAssociatedIcon(path);
        if (icon is null)
        {
            // TODO: standard icon
            return null;
        }

        using var ms = new MemoryStream();
        icon.Save(ms);
        ms.Position = 0;

        var bitmap = new BitmapImage();
        await bitmap.SetSourceAsync(ms.AsRandomAccessStream());

        return bitmap;
    }
}
