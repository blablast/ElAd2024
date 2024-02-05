using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Media.Core;
using Windows.Media.Playback;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Windows.Storage.Streams;
using System.Diagnostics;

namespace ElAd2024.Services;
public partial class CameraService : ObservableRecipient
{
    // TODO: Convert to OpenCV and enable video recording

    public MediaCapture? mediaCapture;
    private LowLagMediaRecording? mediaRecording;

    [ObservableProperty] private bool isRecording = false;
    [ObservableProperty] private string fileName = "video.mp4";
    [ObservableProperty] private int cameraNumber = 0;
    [ObservableProperty] private IMediaPlaybackSource? playbackSource;

    [ObservableProperty] private IReadOnlyList<MediaFrameSourceGroup>? allMediaFrameSourceGroups;
    [ObservableProperty] private MediaFrameSourceGroup? selectedMediaFrameSourceGroup;

    public async Task InitializeAsync()
    {
        AllMediaFrameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
    }
    public async Task<string> StartRecording()
    {
        throw new NotImplementedException();
#pragma warning disable CS0162 // Wykryto nieosiągalny kod
        var myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
        var file = await myVideos.SaveFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
        mediaRecording = await mediaCapture?.PrepareLowLagRecordToStorageFileAsync(
            MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
        await mediaRecording.StartAsync();
        Debug.WriteLine($"Recording to {file.Path}");
        mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
        return file.Name;
#pragma warning restore CS0162 // Wykryto nieosiągalny kod
    }

    public async Task StopRecording()
    {
        throw new NotImplementedException();
#pragma warning disable CS0162 // Wykryto nieosiągalny kod
        if (mediaRecording is not null)
        {
            mediaCapture.RecordLimitationExceeded -= MediaCapture_RecordLimitationExceeded;
            Debug.WriteLine("Stopping recording.");
            await mediaRecording.StopWithResultAsync();
            Debug.WriteLine("Stopped recording.");
            await mediaRecording.FinishAsync();
            Debug.WriteLine("Finished recording.");
        }
#pragma warning restore CS0162 // Wykryto nieosiągalny kod
    }

    private async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
    {
        if (mediaRecording is not null)
        {
            await mediaRecording.StopAsync();
            Debug.WriteLine("Record limitation exceeded.");
        }
    }

    public async Task<string> CapturePhoto(string name = "photo")
    {
        var myPictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
        var file = await myPictures.SaveFolder.CreateFileAsync($"{name}.jpg", CreationCollisionOption.GenerateUniqueName);
        await mediaCapture?.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
        return file.Name;
    }
    public async Task PreviewCamera()
    {
        ArgumentNullException.ThrowIfNull(SelectedMediaFrameSourceGroup);
        mediaCapture = new MediaCapture();
        await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
        {
            SourceGroup = SelectedMediaFrameSourceGroup,
            SharingMode = MediaCaptureSharingMode.SharedReadOnly,
            VideoDeviceId = SelectedMediaFrameSourceGroup.SourceInfos[0].DeviceInformation.Id,
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Auto
        });

        // Set the MediaPlayerElement's Source property to the MediaSource for the mediaCapture.
        PlaybackSource = MediaSource.CreateFromMediaFrameSource(
            mediaCapture.FrameSources[SelectedMediaFrameSourceGroup.SourceInfos[0].Id]);
    }


}
