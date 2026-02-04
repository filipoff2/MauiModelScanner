
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MauiModelScanner.Services;

namespace MauiModelScanner;

public partial class MainPage : ContentPage
{
    private readonly OnnxService _onnx;
    private readonly string _modelsDir = Path.Combine(FileSystem.AppDataDirectory, "Models");

    public ObservableCollection<string> Models { get; } = new();
    private string? _selectedModelPath;

    public MainPage(OnnxService onnx)
    {
        InitializeComponent();
        _onnx = onnx;
        Directory.CreateDirectory(_modelsDir);
        LoadModelsToPicker();
    }

    private void LoadModelsToPicker()
    {
        Models.Clear();
        ModelPicker.Items.Clear();
        var files = Directory.Exists(_modelsDir)
            ? Directory.EnumerateFiles(_modelsDir, "*.onnx").OrderBy(f => f)
            : Enumerable.Empty<string>();
        foreach (var f in files)
        {
            Models.Add(f);
            ModelPicker.Items.Add(Path.GetFileName(f));
        }
        if (ModelPicker.Items.Count > 0)
            ModelPicker.SelectedIndex = 0;
    }

    private async void OnUploadModelClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select ONNX model",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] {"application/octet-stream", "application/onnx", "*/*"} }
                })
            });

            if (result == null)
                return;

            var dest = Path.Combine(_modelsDir, Path.GetFileName(result.FullPath));
            await using (var src = await result.OpenReadAsync())
            await using (var dst = File.Create(dest))
                await src.CopyToAsync(dst);

            // Optional: look for labels.txt next to the model and copy as well
            var labelCandidate = Path.ChangeExtension(result.FullPath, ".labels.txt");
            if (File.Exists(labelCandidate))
            {
                File.Copy(labelCandidate, Path.Combine(_modelsDir, Path.GetFileName(labelCandidate)), overwrite: true);
            }

            LoadModelsToPicker();
            await DisplayAlert("Model uploaded", Path.GetFileName(dest), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Upload failed", ex.Message, "OK");
        }
    }

    private void OnModelSelected(object? sender, EventArgs e)
    {
        if (ModelPicker.SelectedIndex >= 0 && ModelPicker.SelectedIndex < Models.Count)
        {
            _selectedModelPath = Models[ModelPicker.SelectedIndex];
        }
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
                    await DisplayAlert("Permission", "Camera permission denied.", "OK");
                    CameraToggle.IsToggled = false;
                    return;
                }
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Camera.StartCameraPreview(cts.Token);
            }
            else
            {
                Camera.StopCameraPreview();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Camera error", ex.Message, "OK");
            CameraToggle.IsToggled = false;
        }
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        if (_selectedModelPath is null)
        {
            await DisplayAlert("No model", "Please upload and select a model (.onnx)", "OK");
            return;
        }
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            // Capture still image from preview as a Stream (CameraView API)
            using var photoStream = await Camera.CaptureImage(cts.Token);
            if (photoStream == null)
            {
                await DisplayAlert("Capture", "No frame captured.", "OK");
                return;
            }
            photoStream.Position = 0;
            var result = await _onnx.PredictAsync(photoStream, _selectedModelPath);
            ResultLabel.Text = result;
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        ResultLabel.Text = "(no scan yet)";
    }
}
