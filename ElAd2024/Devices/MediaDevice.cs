using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using System.Reflection.Metadata.Ecma335;

namespace ElAd2024.Devices;

public partial class MediaDevice : ObservableRecipient, IMediaDevice
{
    private LowLagMediaRecording? mediaRecording;

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

    public async Task<string> StartRecording(string name)
    {
        if (MediaCapture == null) { return string.Empty; }
        var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);
        var file = await myVideos.SaveFolder.CreateFileAsync($"{name}.mp4", CreationCollisionOption.GenerateUniqueName);

        mediaRecording = await MediaCapture.PrepareLowLagRecordToStorageFileAsync(
                MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
        MediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
        await mediaRecording.StartAsync();
        return file.Name;
    }

    private async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
    {
        if (mediaRecording != null)
        {
            await StopRecording();
            Debug.WriteLine("Record limitation exceeded.");
        }
    }

    public async Task StopRecording()
    {
        if (mediaRecording != null)
        {
            ArgumentNullException.ThrowIfNull(MediaCapture);
            MediaCapture.RecordLimitationExceeded -= MediaCapture_RecordLimitationExceeded;
            await mediaRecording.StopAsync();
            await mediaRecording.FinishAsync();
        }
    }

    public async Task<(string fileName, string fullPath)> CapturePhoto(string name = "photo")
    {
        if (MediaCapture is null)
        {
            return (string.Empty, string.Empty);
        }

        var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
        var file = await myPictures.SaveFolder.CreateFileAsync($"{name}.jpg", CreationCollisionOption.GenerateUniqueName);

        using var captureStream = new InMemoryRandomAccessStream();
        await MediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);

        using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
        var decoder = await BitmapDecoder.CreateAsync(captureStream);
        var encoder = await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder);

        var properties = new BitmapPropertySet {
            { "System.Photo.Orientation", new BitmapTypedValue(PhotoOrientation.Normal, PropertyType.UInt16) }
        };
        await encoder.BitmapProperties.SetPropertiesAsync(properties);
        await encoder.FlushAsync();

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
            VideoDeviceId = SelectedMediaFrameSourceGroup.SourceInfos[0].DeviceInformation.Id,
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Auto
        });
        MediaCapture.Failed += MediaCapture_Failed;
        // Set the MediaPlayerElement's Source property to the MediaSource for the mediaCapture.
        //PlaybackSource = MediaSource.CreateFromMediaFrameSource(
        //    mediaCapture.FrameSources[SelectedMediaFrameSourceGroup.SourceInfos[0].Id]);
        IsConnected = true;
    }

    private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
    {
        IsConnected = false;
        Debug.WriteLine($"MediaCapture_Failed: (0x{errorEventArgs.Code:X}) {errorEventArgs.Message}");
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

    public Task GetData(bool forceClear = false) => throw new NotImplementedException();
    public Task Stop() => throw new NotImplementedException();
}