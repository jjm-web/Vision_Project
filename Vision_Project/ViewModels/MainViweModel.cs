using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Vision_Project.Models;

namespace Vision_Project.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // 카메라 및 녹화 제어를 위한 명령
        public ICommand StartCameraCommand { get; }
        public ICommand StopCameraCommand { get; }
        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand StartRCommand { get; }
        private CameraModel _cameraModel; // 카메라 제어 모델
        private BitmapSource _cameraImage; // UI에 표시할 카메라 이미지
        private bool _isCameraRunning; // 카메라가 실행 중인지 여부
        private bool _isRecording; // 현재 녹화 중인지 여부
        private string _recordingFilePath; // 녹화된 파일의 경로
        
        // 녹화 상태에 따라 버튼의 텍스트를 반환
        public string ButtonContent => _isRecording ? "종료" : "시작";

        // UI에 표시할 카메라 이미지 (바인딩됨)
        public BitmapSource CameraImage
        {
            get => _cameraImage;
            private set
            {
                _cameraImage = value;
                OnPropertyChanged(nameof(CameraImage)); // 속성 변경 알림
            }
        }

        // 카메라 시작 버튼 활성화 여부
        public bool IsStartCameraEnabled => !_isCameraRunning;
        // 카메라 종료 버튼 활성화 여부
        public bool IsStopCameraEnabled => _isCameraRunning;
        // 녹화 시작 버튼 활성화 여부
        public bool IsStartRecordingEnabled => _isCameraRunning && !_isRecording;
        // 녹화 종료 버튼 활성화 여부
        public bool IsStopRecordingEnabled => _isCameraRunning && _isRecording;



        // 생성자: 명령 초기화 및 이벤트 핸들러 연결
        public MainViewModel()
        {
            _cameraModel = new CameraModel();
            _cameraModel.FrameReady += OnFrameReady; // 프레임 준비 이벤트 핸들러

            StartCameraCommand = new RelayCommand(StartCamera);
            StopCameraCommand = new RelayCommand(StopCamera);
            StartRecordingCommand = new RelayCommand(StartRecording);
            StopRecordingCommand = new RelayCommand(StopRecording);
            
        }

        // 카메라 시작 메서드
        private void StartCamera()
        {
            try
            {
                _cameraModel.StartCamera(); // 카메라 시작
                _isCameraRunning = true; // 카메라 실행 중 상태로 설정
                OnPropertyChanged(nameof(IsStartCameraEnabled));
                OnPropertyChanged(nameof(IsStopCameraEnabled));
                OnPropertyChanged(nameof(IsStartRecordingEnabled));
                OnPropertyChanged(nameof(IsStopRecordingEnabled));

            }
            catch (Exception ex)
            {
                // 카메라 시작 오류 시 메시지 박스 표시
                MessageBox.Show($"카메라 시작 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 카메라 종료 메서드
        private void StopCamera()
        {
            _cameraModel.StopCamera(); // 카메라 중지
            _cameraImage = null; // 화면의 카메라 이미지 제거
            _isCameraRunning = false; // 카메라 실행 중 상태 해제
            OnPropertyChanged(nameof(IsStartCameraEnabled));
            OnPropertyChanged(nameof(IsStopCameraEnabled));
            OnPropertyChanged(nameof(IsStartRecordingEnabled));
            OnPropertyChanged(nameof(IsStopRecordingEnabled));
        }

        // 녹화 시작 메서드
        private void StartRecording()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"); // 타임스탬프 생성
            _recordingFilePath = Path.Combine(Path.GetTempPath(), $"{timestamp}.mp4"); // 임시 경로에 녹화 파일 저장

            try
            {
                _cameraModel.StartRecording(_recordingFilePath); // 녹화 시작
                _isRecording = true; // 녹화 중 상태로 설정
                OnPropertyChanged(nameof(IsStartRecordingEnabled));
                OnPropertyChanged(nameof(IsStopRecordingEnabled));
            }
            catch (Exception ex)
            {
                // 녹화 시작 오류 시 메시지 박스 표시
                MessageBox.Show($"녹화 시작 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 녹화 중지 메서드
        private void StopRecording()
        {
            _cameraModel.StopRecording(); // 녹화 중지
            _isRecording = false; // 녹화 중 상태 해제
            PromptToSaveOrCancel(); // 저장 또는 삭제 선택 창 표시
            OnPropertyChanged(nameof(IsStartRecordingEnabled));
            OnPropertyChanged(nameof(IsStopRecordingEnabled));
        }

        // 녹화 중지 후 저장 또는 삭제 여부를 묻는 메서드
        private void PromptToSaveOrCancel()
        {
            var result = MessageBox.Show(
                "동영상을 종료합니다. 동영상을 저장하시겠습니까?", // 메시지 내용
                "저장 여부", // 메시지 제목
                MessageBoxButton.YesNo, // 버튼 옵션
                MessageBoxImage.Question // 아이콘
            );

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveRecording(); // 사용자가 "예"를 선택하면 저장
                    break;

                case MessageBoxResult.No:
                    DeleteRecording(); // 사용자가 "아니오"를 선택하면 삭제
                    break;
            }
        }

        // 녹화된 파일을 저장하는 메서드
        private void SaveRecording()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "MP4 파일 (*.mp4)|*.mp4", // 파일 형식 필터
                FileName = Path.GetFileName(_recordingFilePath) // 기본 파일명 설정
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var savePath = saveFileDialog.FileName;
                try
                {
                    File.Copy(_recordingFilePath, savePath, true); // 녹화된 파일을 선택한 경로에 저장
                    MessageBox.Show($"동영상이 {savePath}에 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // 파일 저장 오류 시 메시지 박스 표시
                    MessageBox.Show($"파일 저장 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                DeleteRecording(); // 사용자가 파일 저장을 취소하면 녹화 파일 삭제
            }
        }

        // 녹화된 파일을 삭제하는 메서드
        private void DeleteRecording()
        {
            if (File.Exists(_recordingFilePath))
            {
                File.Delete(_recordingFilePath); // 녹화된 파일 삭제
            }
        }

        // 카메라 프레임이 준비될 때 호출되는 메서드 (이벤트 핸들러)
        private void OnFrameReady(System.Drawing.Bitmap bitmap)
        {
            var bitmapSource = BitmapSourceConverter.ToBitmapSource(bitmap); // Bitmap을 BitmapSource로 변환
            bitmapSource.Freeze(); // UI 스레드에서 안전하게 사용할 수 있도록 프리즈
            App.Current.Dispatcher.Invoke(() => CameraImage = bitmapSource); // UI 스레드에서 CameraImage 업데이트
        }

        // 속성 변경 알림 메서드 (INotifyPropertyChanged 인터페이스 구현)
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 속성 변경 알림을 위한 이벤트 (INotifyPropertyChanged 인터페이스 구현)
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
