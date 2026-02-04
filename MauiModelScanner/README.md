
# MauiModelScanner (Android)

A .NET MAUI Android sample that lets you:

- **Upload** ONNX AI models at runtime (via file picker)
- **Enable/Disable** the **camera preview** (using CommunityToolkit.Maui.Camera `CameraView`)
- **Select** the active model from a **dropdown** (Picker)
- **Scan from camera**: capture a still frame and run inference with the selected model

## Build

Requirements:
- .NET 8 SDK
- Android SDK/NDK
- Visual Studio 2022 (17.8+) with MAUI workload or `dotnet` CLI

Restore & run on a device:

```bash
dotnet build -t:Run -f net8.0-android
```

## Notes

- CameraView docs show `StartCameraPreview`, `StopCameraPreview`, and `CaptureImage` returning a `Stream`, which we use here. Permissions require `<uses-permission android:name="android.permission.CAMERA"/>` and runtime request. [Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view) 
- ONNX Runtime is cross-platform. On Android, this sample uses the **CPU** execution provider by default via the C# API. Android **NNAPI** acceleration is available in ONNX Runtime Mobile packages and requires registering the NNAPI EP at session creation (availability/config depends on package/build). See the ONNX Runtime **NNAPI EP** documentation. 

## Uploading models

Tap **Upload model** and select a `.onnx` file. Optionally provide a `labels.txt` (or `<model>.labels.txt`) next to it to map class indices to names.

## Inference

Classification-style preprocessing is implemented (224×224, RGB, ImageNet normalize). Adjust in `Services/OnnxService.cs` for your model.

