# On-Device Training Sample (MAUI + Android + LiteRT)

This zip contains:

- **MauiOnDeviceTraining/** – .NET MAUI Android app (UI: camera, collect samples, Train/Infer buttons)
- **AndroidTrainingLib/** – Android Studio Kotlin library that performs **on-device training** using **LiteRT (TensorFlow Lite)** APIs, exported as an **AAR** and called from MAUI via JNI.

## Why LiteRT (TensorFlow Lite)?
- Google supports **on-device training** on Android by exporting multiple **signatures** (e.g., `train`, `infer`, `save`, `restore`) in a `.tflite` model.  
  See the official guide and tutorial: https://ai.google.dev/edge/litert/conversion/tensorflow/build/ondevice_training  
- TensorFlow’s blog introduced on-device training support in TF 2.7+ and shows end‑to‑end Android examples: https://blog.tensorflow.org/2021/11/on-device-training-in-tensorflow-lite.html  

**Note:** ONNX Runtime Mobile supports **inference** on Android but **not training**. Use LiteRT for training on phone; keep MAUI as the UI shell.

## How to use
1. Open **AndroidTrainingLib/** in **Android Studio** and build the AAR in Release.
2. Copy the AAR to `MauiOnDeviceTraining/libs/` and uncomment the `<AndroidLibrary>` entry in the MAUI `.csproj`.
3. Ensure your training-enabled `.tflite` path is provided to `Trainer.configureModelPath(...)` (you can call this via an additional JNI method or set it once from MAUI).
4. Build & deploy the MAUI app to an Android device:
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

## Limitations
- Phone training is feasible for **transfer learning** or **small models**; avoid full end‑to‑end training for large nets.
- Shipping `.tflite` with training ops can increase size; consider downloading it after license/activation.

## Security
- If you sell models, encrypt them, decrypt in memory, and use Android Keystore for key protection. True DRM is not possible on a fully local model, but you can raise the barrier with in‑memory decryption and obfuscation.


# 📱 Maui On‑Device Training Sample

A .NET MAUI (Android) app that lets you:

- **Capture** images from the device **camera**  
- **Collect** them as a small dataset on the phone  
- **Train** an on‑device model (via a native Android library)  
- **Infer** with the updated model—all **on the phone**, no cloud required

> The MAUI UI (C#) handles camera preview, capture, and buttons.  
> A native Android library (Kotlin) performs the actual on‑device training and inference (LiteRT / TensorFlow Lite).

---

## ✨ Features

- **Camera preview & capture** using CommunityToolkit `CameraView`
- **Dataset builder**: save captured frames to the app’s local storage
- **On‑device training**: fine‑tune a **training‑enabled** `.tflite` model with your captured samples
- **Inference**: run predictions with the updated model and display the result
- **Android‑only** (for training): MAUI provides the cross‑platform shell; the training/ML execution is Android‑native

---

## 🧱 Architecture

```
MauiOnDeviceTrainingSample/
│
├─ MauiOnDeviceTraining/          # .NET MAUI app (UI shell)
│  ├─ MainPage.xaml(.cs)          # Camera preview, capture, Train, Infer
│  ├─ Services/Trainer.cs         # JNI bridge to AndroidTrainingLib (AAR)
│  ├─ Resources/Models/           # (optional) seed .tflite files
│  ├─ Resources/Dataset/          # runtime dataset (captured images)
│  └─ Platforms/Android/          # Android manifest & libs
│
└─ AndroidTrainingLib/            # Android Studio library (Kotlin)
   ├─ build.gradle                # depends on LiteRT/TensorFlow Lite
   ├─ src/.../Trainer.kt          # train() & infer() signatures
   └─ (AAR output for MAUI)
```

- **MAUI** (C#): presents UI, uses `CameraView`, saves images, and calls the Kotlin library via **JNI**.
- **AndroidTrainingLib** (Kotlin): loads a **training‑enabled `.tflite`** and executes **`train`** / **`infer`** signatures.

---

## 🧩 What is a “training‑enabled” `.tflite`?

Your model must be exported with multiple **signatures**, typically:

- `train` — updates weights using your examples  
- `infer` — runs forward pass and outputs scores/labels  
- `save` / `restore` — optional, to persist updated weights

You’ll point the Android library at this `.tflite` file. The library handles preprocessing (e.g., resizing to 224×224, normalization) and calls the proper signatures during **Train** and **Infer**.

> **Tip:** Use transfer learning (e.g., train only the final layers) to keep training fast on phones.

---

## ▶️ Running the App (Android)

1. **Build the Android training library (AAR)**  
   Open `AndroidTrainingLib/` in **Android Studio** and build **Release** → produces `AndroidTrainingLib-release.aar`.

2. **Add AAR to MAUI**  
   Copy the AAR to `MauiOnDeviceTraining/libs/` and in `MauiOnDeviceTraining.csproj` **uncomment** the line:
   ```xml
   <AndroidLibrary Include="libs/AndroidTrainingLib-release.aar" />
   ```

3. **Deploy MAUI to your device**
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

4. **Use the app**  
   - Toggle **Camera** → preview starts (permission prompt on first run)  
   - Tap **Capture sample** → image is saved to the dataset  
   - Tap **Train on device** → the Kotlin library runs the `train` signature  
   - Tap **Infer (trained)** → runs `infer` on a new frame and shows the result

---

## ⚙️ Configuration

- **Model path**  
  Provide your `.tflite` to the Android library (e.g., via a config method or by placing it in app storage). The Kotlin side expects a **valid path** to the training‑enabled `.tflite`.

- **Preprocessing**  
  Default: **224×224** RGB with ImageNet‑style normalization. If your model expects different size/mean/std, adjust it in the Android library.

- **Labels**  
  If you use class labels (e.g., `ripe`, `unripe`, `overripe`), keep a consistent mapping between the MAUI UI and the Kotlin one‑hot/decoding logic.

---

## 🔐 Shipping / Security notes

If you plan to **sell models** or ship paid content:

- **Encrypt** your `.tflite`, **decrypt in memory** only  
- Store/derive keys with **Android Keystore**  
- Obfuscate the native loader / split keys  
- Optionally **download** the model post‑license check

> No local protection is absolute, but these steps raise the barrier.

---

## 🧪 Testing checklist

- Camera permission granted and preview starts/stops cleanly  
- At least a few samples captured before training  
- Training runs for the configured epochs without exceptions  
- Inference after training shows a sensible label/score  
- Model path and tensor names match your exported signatures

---

## 🚀 Extending

- **Continuous training**: add a small scheduler to retrain after N new samples  
- **Live classification**: add timed captures during preview (throttled)  
- **Bounding boxes / segmentation**: adapt model + overlay drawing  
- **Model packs**: download model archives + labels at first launch  
- **Telemetry**: log time per epoch, accuracy, loss

---

## 🧠 Summary

This sample demonstrates a practical pattern for **on‑device model personalization**:

- MAUI provides a familiar C# UI and camera integration  
- A native Android library performs **real training** (update weights) and **inference**  
- The phone can learn from the user’s data locally—no uploads needed

