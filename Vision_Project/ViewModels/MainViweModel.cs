using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Vision_Project.Models;

namespace Vision_Project.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand ToggleRecordingCommand { get; }

        private CameraModel _cameraModel;
        private BitmapSource _cameraImage;
        private bool _isRecording;

        public string ButtonContent
        {
            get => _isRecording ? "종료" : "시작";
        }

        public BitmapSource CameraImage
        {
            get => _cameraImage;
            private set
            {
                _cameraImage = value;
                OnPropertyChanged(nameof(CameraImage));
            }
        }

        public MainViewModel()
        {
            _cameraModel = new CameraModel();
            _cameraModel.FrameReady += OnFrameReady;

            ToggleRecordingCommand = new RelayCommand(ToggleRecording);
        }

        private void ToggleRecording()
        {
            try
            {
                if (_isRecording)
                {
                    _cameraModel.StopRecording();
                }
                else
                {
                    _cameraModel.StartRecording();
                }
                _isRecording = !_isRecording;
                OnPropertyChanged(nameof(ButtonContent));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"카메라를 열 수 없습니다. 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnFrameReady(Bitmap bitmap)
        {
            var bitmapSource = BitmapSourceConverter.ToBitmapSource(bitmap);
            bitmapSource.Freeze(); // UI 스레드에서 사용하기 위해 프리즈
            App.Current.Dispatcher.Invoke(() => CameraImage = bitmapSource);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
