using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using MauiOnDeviceTraining.Services;

namespace MauiOnDeviceTraining;

public partial class MainPage : ContentPage
{
    private readonly ITrainer _trainer;
    private readonly string _datasetDir = Path.Combine(FileSystem.AppDataDirectory, "Dataset");

    public MainPage(ITrainer trainer)
    {
        InitializeComponent();
        _trainer = trainer;
        Directory.CreateDirectory(_datasetDir);
    }

    private async void OnCameraToggled(object? sender, ToggledEventArgs e)
    {
        try
        {
            if (e.Value)
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission", "Camera permission denied", "OK");
                    CameraToggle.IsToggled = false; return;
                }
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Camera.StartCameraPreview(cts.Token);
            }
            else { Camera.StopCameraPreview(); }
        }
        catch (Exception ex) { await DisplayAlert("Camera", ex.Message, "OK"); CameraToggle.IsToggled = false; }
    }

    private async void OnCaptureSample(object? sender, EventArgs e)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var stream = await Camera.CaptureImage(cts.Token);
            if (stream == null) { StatusLabel.Text = "No frame"; return; }
            var file = Path.Combine(_datasetDir, $"sample_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            using var fs = File.Create(file);
            await stream.CopyToAsync(fs);
            StatusLabel.Text = $"Saved: {Path.GetFileName(file)}";
        }
        catch (Exception ex) { StatusLabel.Text = ex.Message; }
    }

    private async void OnTrainClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Training...";
            var result = await _trainer.TrainAsync(_datasetDir);
            StatusLabel.Text = result;
        }
        catch (Exception ex) { StatusLabel.Text = ex.Message; }
    }

    private async void OnInferClicked(object? sender, EventArgs e)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var stream = await Camera.CaptureImage(cts.Token);
            if (stream == null) { StatusLabel.Text = "No frame"; return; }
            var tmp = Path.Combine(FileSystem.CacheDirectory, "infer.jpg");
            using (var fs = File.Create(tmp)) { await stream.CopyToAsync(fs); }
            var result = await _trainer.InferAsync(tmp);
            StatusLabel.Text = result;
        }
        catch (Exception ex) { StatusLabel.Text = ex.Message; }
    }
}
