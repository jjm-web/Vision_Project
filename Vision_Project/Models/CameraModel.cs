using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Vision_Project.Models
{
    public class CameraModel : IDisposable
    {
        private VideoCapture _capture;
        private bool _isRecording;
        public event Action<Bitmap> FrameReady;

        public void StartRecording()
        {
            _capture = new VideoCapture(0);
            if (!_capture.IsOpened())
            {
                throw new Exception("카메라를 열 수 없습니다.");
            }
            _isRecording = true;
            Task.Run(() => CaptureFrames());
        }

        private void CaptureFrames()
        {
            while (_isRecording)
            {
                using (var frame = _capture.RetrieveMat())
                {
                    if (frame.Empty())
                        continue;

                    var bitmap = BitmapConverter.ToBitmap(frame);
                    FrameReady?.Invoke(bitmap);
                }
            }
        }

        public void StopRecording()
        {
            _isRecording = false;
            _capture?.Release();
        }

        public void Dispose()
        {
            StopRecording();
            _capture?.Dispose();
        }
    }
}
