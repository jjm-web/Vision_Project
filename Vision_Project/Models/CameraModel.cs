using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Basler.Pylon;

namespace Vision_Project.Models
{
    public class CameraModel : IDisposable
    {
        private Camera _camera; // Basler 카메라 객체
        private bool _isRecording; // 녹화 상태를 나타내는 플래그
        private string _recordingFilePath; // 녹화 파일 경로
        private int _frameCount = 0; // 녹화된 프레임 수
        private const string FileExtension = ".bmp"; // 이미지 파일 확장자
        private CancellationTokenSource _cancellationTokenSource; // 취소 토큰 소스

        public event Action<Bitmap> FrameReady; // 프레임 준비 이벤트

    public void StartCamera()
    {

        try
        {
             if (_camera == null)
             {

                    _camera = new Camera(); // 기본 카메라 인스턴스
                    _camera.CameraOpened += Configuration.AcquireContinuous;
                    _camera.Open();

                    // 카메라 설정
                    _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                    _camera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(5); // 버퍼 수 설정
                }

              if (!_camera.StreamGrabber.IsGrabbing)
              {
                    _camera.StreamGrabber.Start();
                    // CancellationTokenSource를 생성하고 CaptureFrames 호출
                    _cancellationTokenSource = new CancellationTokenSource();
                    Task.Run(() => CaptureFrames(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
              }
         }
        catch (Exception ex)
        {
            Console.WriteLine($"카메라를 열 수 없습니다: {ex.Message}");
            throw;
        }
    }
        
        private void CaptureFrames(CancellationToken token)
        {
            while (_camera != null && _camera.StreamGrabber.IsGrabbing && !token.IsCancellationRequested)
            {
                IGrabResult grabResult = null;

                try
                {
                    grabResult = _camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                    if (grabResult == null)
                    {
                        Console.WriteLine("Error: GrabResult is null");
                        continue;
                    }

                    if (grabResult.GrabSucceeded)
                    {
                        // 이미지 처리
                        byte[] pixelData = grabResult.PixelData as byte[];

                        if (pixelData != null)
                        {
                            int width = grabResult.Width;
                            int height = grabResult.Height;

                            using (var bitmap = new Bitmap(width, height, width, PixelFormat.Format8bppIndexed, IntPtr.Zero))
                            {
                                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                                IntPtr scan0 = bitmapData.Scan0;
                                System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, scan0, pixelData.Length);
                                bitmap.UnlockBits(bitmapData);

                                // 프레임 준비 이벤트 호출
                                FrameReady?.Invoke(bitmap);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
                finally
                {
                    grabResult?.Dispose();
                }
            }

            // 메모리 정리
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

       
        private void SaveFrameToFile(Bitmap bitmap)
        {
            if (string.IsNullOrEmpty(_recordingFilePath))
            {
                throw new InvalidOperationException("녹화 파일 경로가 설정되지 않았습니다.");
            }

            string fileName = Path.Combine(_recordingFilePath, $"{_frameCount:D4}{FileExtension}");
            bitmap.Save(fileName, ImageFormat.Bmp);
            _frameCount++;
        }

        public void StartRecording(string directoryPath)
        {
            if (_camera == null || !_camera.StreamGrabber.IsGrabbing)
            {
                throw new InvalidOperationException("카메라가 시작되지 않았습니다. 먼저 StartCamera를 호출하세요.");
            }

            _recordingFilePath = directoryPath;

            if (!Directory.Exists(_recordingFilePath))
            {
                Directory.CreateDirectory(_recordingFilePath);
            }

            _isRecording = true;
        }

        public void StopRecording()
        {
            _isRecording = false;
        }
        public void StopCamera()
        {
            try
            {
                if (_camera != null && _camera.StreamGrabber.IsGrabbing)
                {
                    _camera.StreamGrabber.Stop();
                }

                // CancellationTokenSource 취소
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _camera.StreamGrabber.Stop();
                _camera.Close();
            
                _camera = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"카메라를 멈추는 동안 오류가 발생했습니다: {ex.Message}");
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        public void Dispose()
        {
            StopRecording();
    
        }
    }
}
