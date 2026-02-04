# MauiModelScanner (Android) — Project Guide

This document explains **what the app does**, **how it’s structured**, and **how to run and extend it** on an **Android phone**. It accompanies the sample project I shared with you (`MauiModelScanner.zip`).

> **Quick pitch:** A .NET MAUI Android app where you can **upload ONNX models at runtime**, **toggle the camera** on/off, pick the **active model** from a dropdown, and **scan** the current camera frame to get a prediction.

---

## 1) Features

- **Upload AI models** at runtime using the Android file picker (accepts `*.onnx`). The app copies the selected model into its app data folder and refreshes the dropdown.
- **Model selector (dropdown)** shows all uploaded `.onnx` files; the selected one becomes the **active** model for inference.
- **Camera ON/OFF** switch starts/stops the **live preview** using the Community Toolkit **CameraView** control. (Requires CAMERA permission.) citeturn12search59
- **Scan from camera** button captures a still frame from the preview and feeds it to the active model.
- **Labels support**: place a `labels.txt` (or `<model>.labels.txt`) next to your model to map class indices → names in the result display.

> CameraView documentation (preview, capture, flash, resolutions) lives in the .NET MAUI Community Toolkit docs. citeturn12search59

---

## 2) High‑level architecture

```text
MainPage (UI)
 ├─ Upload model (FilePicker)
 ├─ Picker of models (from AppData/Models)
 ├─ Camera toggle (CameraView.Start/Stop preview)
 └─ Scan button → Camera.CaptureImage() → OnnxService.PredictAsync()

Services/OnnxService
 ├─ Manages cached ONNX InferenceSession per model path
 ├─ Preprocesses camera frame to [1,3,224,224] (RGB, ImageNet mean/std)
 └─ Runs inference with ONNX Runtime → Softmax → Top‑1 (+ labels mapping)
```

- **CameraView** comes from `CommunityToolkit.Maui.Camera` and supports starting/stopping preview and capturing an image as a `Stream`. citeturn12search59
- **ONNX Runtime** is used for inference. On Android, it runs on **CPU** by default via the C# API; you may enable Android **NNAPI** execution provider (EP) with the appropriate package/build to access device accelerators. citeturn5search28turn12search68

---

## 3) Project layout

```
MauiModelScanner/
  MauiModelScanner.csproj          # net8.0-android, package refs
  App.xaml / App.xaml.cs           # App bootstrap
  MauiProgram.cs                   # Registers Toolkit, SkiaSharp, DI services
  MainPage.xaml / .cs              # UI: upload, picker, toggle camera, scan
  Services/OnnxService.cs          # ONNX inference + preprocessing
  Platforms/Android/AndroidManifest.xml  # CAMERA permission
  Resources/Models/                # (optional) starter models folder
  README.md                        # quick instructions in the project
```

---

## 4) Build & deploy (Android)

**Prerequisites**
- .NET 8 SDK and MAUI workload
- Android SDK/NDK
- An Android device with USB debugging enabled

**Run**
```bash
dotnet build -t:Run -f net8.0-android
```

Open the app on your phone after deployment.

---

## 5) Using the app

1. **Upload model** (ONNX): Tap **Upload model** and pick a `*.onnx` file from device storage. The app copies it to `AppData/Models` and refreshes the dropdown.
2. **(Optional) Labels**: Put a `labels.txt` (or `<model>.labels.txt`) next to the model before uploading. The app reads it to display friendly class names.
3. **Enable camera**: Flip the switch to request permission and start the live preview (CameraView). You can stop it by toggling off. citeturn12search59
4. **Scan from camera**: Press **Scan** to capture a frame and run inference using the selected model.

---

## 6) Model assumptions

- The default preprocessor resizes to **224×224**, converts to **RGB**, and applies **ImageNet mean/std**. This matches many classification backbones (e.g., MobileNetV2). If your model expects a different size or normalization, edit `Services/OnnxService.cs` accordingly.
- Output is read as a **1D float vector** and passed through **softmax**; the top‑1 label and probability are shown.

> You can swap in object detection or other heads by adjusting pre/post‑processing and interpreting the outputs differently.

---

## 7) Acceleration (optional)

The sample uses ONNX Runtime **CPU** by default. On Android you can enable **NNAPI** EP to access phone accelerators (GPU/NPU) when supported. Enabling NNAPI requires using the Android/mobile ONNX Runtime build and registering the NNAPI EP when creating the session (e.g., via native/Java bindings or a package that exposes it). See the ONNX Runtime **NNAPI** documentation for requirements and flags (e.g., `NNAPI_FLAG_USE_FP16`). citeturn12search68turn12search70

---

## 8) Permissions

The app declares the **CAMERA** permission in `Platforms/Android/AndroidManifest.xml` and also requests it at runtime. Camera APIs are provided by the MAUI Community Toolkit **CameraView**. citeturn12search59

---

## 9) Troubleshooting

- **Black preview / errors**: Ensure **camera permission** is granted and another app isn’t holding the camera. Toggle OFF/ON to restart preview. (CameraView supports `StartCameraPreview`/`StopCameraPreview`.) citeturn12search59
- **Model loads but predictions look wrong**: Verify **input size** and **normalization** match your training pipeline; adjust `OnnxService.PreprocessToCHW`.
- **Need more speed**: Consider **quantized models** or enable **NNAPI** EP per ONNX Runtime docs. citeturn12search68

---

## 10) Extending the sample

- **Continuous scanning**: start a timer while preview is ON and call `CaptureImage` at intervals; debounce to keep UI responsive.
- **Bounding boxes**: overlay results using MAUI drawing or SkiaSharp canvas.
- **Format support**: in `OnnxService`, adapt preprocessing for grayscale/BGR/float16, etc.
- **Model packs**: ship models as MAUI assets (`Resources/Models`) and load them on first run.

---

## References
- **CameraView** (CommunityToolkit.MAUI) — preview, capture, flash, resolutions: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view citeturn12search59
- **ONNX Runtime** — cross‑platform runtime; Android **NNAPI EP** overview & build/usage: https://onnxruntime.ai/docs/execution-providers/NNAPI-ExecutionProvider.html citeturn12search68
