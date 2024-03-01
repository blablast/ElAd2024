using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Playback;

namespace ElAd2024.Contracts.Devices;
public interface IMediaDevice : IDevice
{
    int CameraNumber { get; set; }
    bool IsRecording { get; }
    IMediaPlaybackSource? PlaybackSource { get;}
    MediaFrameSourceGroup? SelectedMediaFrameSourceGroup {
        get; set;
    }
    MediaCapture? MediaCapture { get; }
    Task<IReadOnlyList<MediaFrameSourceGroup>> AllMediaFrameSourceGroups();
    Task<(string fileName, string fullPath)> CapturePhoto(string name);
    Task StopRecording();
    Task<string> StartRecording(string name);
}
