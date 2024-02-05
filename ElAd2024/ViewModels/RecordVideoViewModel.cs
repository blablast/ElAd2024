using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace ElAd2024.ViewModels;
public partial class RecordVideoViewModel : ObservableRecipient
{
    [ObservableProperty] private bool isRecording = false;
    [ObservableProperty] private string fileName = string.Empty;
    [ObservableProperty] private MediaCapture? mediaCapture;
    private LowLagMediaRecording? mediaRecording;

    public async Task Start()
    {
        if (MediaCapture is not null && !IsRecording)
        {
            var myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            var file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            mediaRecording = await MediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p), file);
            FileName = file.Name;
            IsRecording = true;
            await mediaRecording?.StartAsync();
        }
        else
        {
            throw new InvalidOperationException("Cannot start recording when MediaCapture is not provided.");
        }
    }

    public async Task Pause()
    {
        if (mediaRecording is not null && IsRecording)
        {
            await mediaRecording.StopAsync();
            IsRecording = false;
        }
        else
        {
            throw new InvalidOperationException("Cannot pause recording when not recording.");
        }
    }

    public async Task Resume()
    {
        if (mediaRecording is not null && IsRecording)
        {
            await mediaRecording.StopAsync();
            IsRecording = true;
        }
        else
        {
            throw new InvalidOperationException("Cannot resume recording when not recording.");
        }
    }

    public async Task Finish()
    {
        if (mediaRecording is not null && IsRecording)
        {
            await mediaRecording.FinishAsync();
            IsRecording = false;
        }
        else
        {
            throw new InvalidOperationException("Cannot finish recording when not recording.");
        }
    }

}
