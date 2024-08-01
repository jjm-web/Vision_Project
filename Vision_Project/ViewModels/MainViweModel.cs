using System.ComponentModel;
using System.Windows.Input;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Drawing;
using CommunityToolkit.Mvvm.Input;
using Vision_Project;

public class MainViewModel : INotifyPropertyChanged
{
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    private VideoCapture _capture;
    private bool _isRecording;

    public MainViewModel()
    {
        StartCommand = new RelayCommand(StartRecording);
        StopCommand = new RelayCommand(StopRecording);
    }

    private void StartRecording()
    {
        _capture = new VideoCapture(0); // 내장된 카메라 사용
        _isRecording = true;
        Task.Run(() =>
        {
            while (_isRecording)
            {
                using (var frame = _capture.RetrieveMat())
                {
                    Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                    var bitmapSource = BitmapSourceConverter.ToBitmapSource(bitmap);
                    bitmapSource.Freeze(); // UI 스레드에서 사용하기 위해 프리즈
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ((MainWindow)App.Current.MainWindow).CameraImage.Source = bitmapSource;
                    });
                }
            }
        });
    }

    private void StopRecording()
    {
        _isRecording = false;
        _capture.Release();
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
