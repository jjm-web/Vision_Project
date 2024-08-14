using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Vision_Project.Models
{
    public class CameraModel : IDisposable
    {
        private VideoCapture _capture; // 카메라 캡처를 담당하는 객체
        private bool _isRecording; // 현재 녹화 중인지 여부를 나타내는 플래그
        private VideoWriter _writer; // 비디오 녹화를 담당하는 객체
        private string _recordingFilePath; // 녹화된 파일의 경로
        public event Action<Bitmap> FrameReady; // 프레임이 준비되었을 때 발생하는 이벤트

        // 카메라를 시작하는 메서드
        public void StartCamera()
        {
            // 이미 카메라가 시작된 경우 중복으로 시작하지 않도록 방지
            if (_capture != null && _capture.IsOpened())
                return;

            _capture = new VideoCapture(0); // 카메라를 초기화 (0은 기본 카메라를 의미)
            
            if (!_capture.IsOpened())
            {
                throw new Exception("카메라를 열 수 없습니다."); // 카메라를 열 수 없을 때 예외 발생
            }

            Task.Run(() => CaptureFrames()); // 비동기적으로 프레임 캡처를 시작
        }

        // 녹화를 시작하는 메서드
        public void StartRecording(string filePath)
        {
            // 카메라가 시작되지 않았을 경우 예외 발생
            if (_capture == null || !_capture.IsOpened())
            {
                throw new InvalidOperationException("카메라가 시작되지 않았습니다. 먼저 StartCamera를 호출하세요.");
            }

            _recordingFilePath = filePath; // 녹화 파일의 경로 설정

            // 프레임의 가로 및 세로 크기를 설정
            var frameWidth = _capture.FrameWidth;
            var frameHeight = _capture.FrameHeight;

            // 비디오 파일을 작성하는 객체를 초기화
            _writer = new VideoWriter(filePath, FourCC.MP4V, 30, new OpenCvSharp.Size(frameWidth, frameHeight));
            if (!_writer.IsOpened())
            {
                throw new Exception("비디오 파일을 열 수 없습니다."); // 비디오 파일을 열 수 없을 때 예외 발생
            }

            _isRecording = true; // 녹화 상태를 true로 설정
        }

        // 프레임을 캡처하는 메서드 (비동기로 실행됨)
        private void CaptureFrames()
        {
            while (_capture != null && _capture.IsOpened())
            {
                using (var frame = new Mat()) // 새 프레임을 저장할 Mat 객체 생성
                {
                    _capture.Read(frame); // 카메라에서 프레임을 읽음
                    if (frame.Empty())
                        continue; // 프레임이 비어 있을 경우 다음 루프 반복

                    // 녹화 중일 때 프레임을 비디오 파일에 기록
                    if (_isRecording && _writer != null)
                    {
                        try
                        {
                            lock (_writer) // 스레드 간 충돌 방지
                            {
                                _writer.Write(frame); // 프레임을 비디오 파일에 기록
                            }
                        }
                        catch (AccessViolationException ex)
                        {
                            // 비디오 파일 작성 중 오류가 발생할 경우 예외 처리
                            Console.WriteLine("AccessViolationException 발생: " + ex.Message);
                            StopRecording(); // 문제가 발생하면 녹화를 중단
                        }
                    }
                    // 프레임을 Bitmap으로 변환하고 FrameReady 이벤트를 발생시킴
                    var bitmap = BitmapConverter.ToBitmap(frame);
                    FrameReady?.Invoke(bitmap);
                }
            }
        }

        // 녹화를 중지하는 메서드
        public void StopRecording()
        {
            lock (_writer) // 스레드 간 충돌 방지
            {
                _isRecording = false; // 녹화 상태를 false로 설정
                _writer?.Release(); // 비디오 파일 작성기를 해제
                _writer = null; // 비디오 작성 객체를 null로 설정
            }
        }

        // 카메라를 중지하는 메서드
        public void StopCamera()
        {
            lock (_capture) // 스레드 간 충돌 방지
            {
                _isRecording = false; // 녹화 상태를 false로 설정
                _capture?.Release(); // 카메라 객체를 해제
                _capture = null; // 카메라 객체를 null로 설정
            }
        }

        // 리소스를 해제하는 메서드 (IDisposable 인터페이스 구현)
        public void Dispose()
        {
            StopRecording(); // 녹화 중지
            StopCamera(); // 카메라 중지
        }
    }
}