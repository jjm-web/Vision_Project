using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace Vision_Project.Models
{
    public class CameraModel : IDisposable
    {
        private VideoCapture _videoCapture;
        private bool _isRecording;

        public CameraModel()
        {
            _videoCapture = new VideoCapture(0); // 0 is the default camera
            if (!_videoCapture.IsOpened())
            {
                throw new Exception("Could not open video capture device.");
            }
        }

        public async Task StartRecordingAsync(Action<Mat> frameCallback)
        {
            _isRecording = true;

            await Task.Run(() =>
            {
                using var frame = new Mat();
                while (_isRecording)
                {
                    if (_videoCapture.Read(frame) && !frame.Empty())
                    {
                        // Call the callback method with the cloned frame
                        frameCallback?.Invoke(frame.Clone());
                    }
                }
            });
        }

        public void StopRecording()
        {
            _isRecording = false;
        }

        public void Dispose()
        {
            _videoCapture?.Release();
            _videoCapture?.Dispose();
        }
    }
}
