using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices;

public partial class CameraDevice : ObservableRecipient, ICameraDevice
{
    [ObservableProperty] private int cameraNumber;

    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private bool isSimulated = false;

    [ObservableProperty] private bool isRecording;
    // TODO: Convert to OpenCV and enable video recording

    [ObservableProperty] private MediaCapture? mediaCapture;
    [ObservableProperty] private IMediaPlaybackSource? playbackSource;

    [ObservableProperty] private MediaFrameSourceGroup? selectedMediaFrameSourceGroup;

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<MediaFrameSourceGroup>> AllMediaFrameSourceGroups() => await MediaFrameSourceGroup.FindAllAsync();

    public async Task<string> StartRecording()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
        //var file = await myVideos.SaveFolder.CreateFileAsync(VideoFileName, CreationCollisionOption.GenerateUniqueName);
        //mediaRecording = await MediaCapture?.PrepareLowLagRecordToStorageFileAsync(
        //    MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
        //await mediaRecording.StartAsync();
        //Debug.WriteLine($"Recording to {file.Path}");
        //MediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
        //return file.Name;
    }

    public async Task StopRecording()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (mediaRecording is not null)
        //{
        //    MediaCapture.RecordLimitationExceeded -= MediaCapture_RecordLimitationExceeded;
        //    Debug.WriteLine("Stopping recording.");
        //    await mediaRecording.StopWithResultAsync();
        //    Debug.WriteLine("Stopped recording.");
        //    await mediaRecording.FinishAsync();
        //    Debug.WriteLine("Finished recording.");
        //}
    }

    //private async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
    //{
    //    if (mediaRecording is not null)
    //    {
    //        await mediaRecording.StopAsync();
    //        Debug.WriteLine("Record limitation exceeded.");
    //    }
    //}

    public async Task<(string fileName, string fullPath)> CapturePhoto(string name = "photo")
    {
        var myPictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
        var file = await myPictures.SaveFolder.CreateFileAsync($"{name}.jpg",
            CreationCollisionOption.GenerateUniqueName);
        await MediaCapture?.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
        return (file.Name, file.Path);
    }

    public async Task ConnectAsync(object? parameters = null)
    {
        IsConnected = false;
        ArgumentNullException.ThrowIfNull(SelectedMediaFrameSourceGroup);
        MediaCapture = new MediaCapture();
        await MediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
        {
            SourceGroup = SelectedMediaFrameSourceGroup,
            SharingMode = MediaCaptureSharingMode.SharedReadOnly,
            VideoDeviceId = SelectedMediaFrameSourceGroup.SourceInfos[0].DeviceInformation.Id,
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Auto
        });

        // Set the MediaPlayerElement's Source property to the MediaSource for the mediaCapture.
        //PlaybackSource = MediaSource.CreateFromMediaFrameSource(
        //    mediaCapture.FrameSources[SelectedMediaFrameSourceGroup.SourceInfos[0].Id]);
        IsConnected = true;
    }
    public Task DisconnectAsync()
    {
        MediaCapture?.Dispose();
        MediaCapture = null;
        IsConnected = false;
        return Task.CompletedTask;
    }
    public void Dispose()
    {
        DisconnectAsync();
        GC.SuppressFinalize(this);
    }

}