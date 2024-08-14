using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public static class BitmapSourceConverter
{
    // Bitmap을 BitmapSource로 변환하는 메서드
    public static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        // 메모리 스트림을 사용하여 Bitmap 데이터를 읽고, 이를 BitmapSource로 변환
        using (var memoryStream = new MemoryStream())
        {
            // Bitmap을 메모리 스트림에 BMP 포맷으로 저장
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

            // 메모리 스트림의 위치를 처음으로 설정 (이미지를 처음부터 읽기 위해)
            memoryStream.Position = 0;

            // BitmapSource 객체를 생성하고 초기화
            var bitmapSource = new BitmapImage();
            bitmapSource.BeginInit(); // 초기화 시작
            bitmapSource.StreamSource = memoryStream; // 메모리 스트림을 데이터 소스로 설정
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad; // 이미지를 로드할 때 메모리에 캐시
            bitmapSource.EndInit(); // 초기화 종료

            // BitmapSource 객체를 반환
            return bitmapSource;
        }
    }
}
