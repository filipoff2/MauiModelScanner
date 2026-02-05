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
