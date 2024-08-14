using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

public static class BitmapSourceConverter
{
    public static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        try
        {
            using (var stream = new MemoryStream())
            {
                // Bitmap을 BMP 형식으로 메모리 스트림에 저장
                bitmap.Save(stream, ImageFormat.Bmp);

                // 스트림을 처음 위치로 되돌리기
                stream.Seek(0, SeekOrigin.Begin);

                // BitmapImage 객체를 생성
                var bitmapImage = new BitmapImage();

                // BitmapImage 초기화
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream; // 메모리 스트림을 소스에 설정
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 캐시 옵션 설정
                bitmapImage.EndInit();

                // BitmapSource 반환
                return bitmapImage;
            }
        }
        catch (Exception ex)
        {
            // 예외 처리: 적절한 예외 처리 로직을 추가
            Console.WriteLine($"Error converting Bitmap to BitmapSource: {ex.Message}");
            throw;
        }
    }
}
