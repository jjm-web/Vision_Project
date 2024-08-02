using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public static class BitmapSourceConverter
{
    public static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        using (var memoryStream = new MemoryStream())
        {
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Position = 0;
            var bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.StreamSource = memoryStream;
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.EndInit();
            return bitmapSource;
        }
    }
}
