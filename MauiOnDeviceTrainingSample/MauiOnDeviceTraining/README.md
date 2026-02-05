# MauiOnDeviceTraining (Android) – MAUI front-end

This MAUI app captures samples via **CameraView**, and exposes buttons to **Train on device** and **Infer (trained)**. The actual on-device training is done by an Android library (Kotlin) using **LiteRT (TensorFlow Lite)** on-device training APIs and invoked via JNI from C#.

**Why LiteRT?** On-device training is supported by TensorFlow Lite / LiteRT for Android (multi-signature `.tflite` models with train/infer/save/restore). ONNX Runtime Mobile supports inference only; not training on Android. See references below. 

- LiteRT on-device training overview and tutorial (multi-signature `.tflite`, train/infer/save/restore)  
  https://ai.google.dev/edge/litert/conversion/tensorflow/build/ondevice_training  
- TensorFlow blog: *On-device training in TensorFlow Lite* (Android support, TF ≥2.7)  
  https://blog.tensorflow.org/2021/11/on-device-training-in-tensorflow-lite.html  

## How to wire the trainer AAR
1. Open **AndroidTrainingLib** (sibling folder) in **Android Studio** and build the AAR (Release).
2. Copy `AndroidTrainingLib/build/outputs/aar/AndroidTrainingLib-release.aar` into `MauiOnDeviceTraining/libs/` (create folder if needed).
3. In `MauiOnDeviceTraining.csproj`, **uncomment** the `<AndroidLibrary Include="libs/AndroidTrainingLib-release.aar" />` item.
4. Build & deploy the MAUI app to an **Android phone**:
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

## Using the app
- Toggle **Camera** to start preview (grants CAMERA permission).
- Tap **Capture sample** to collect images; they are saved under the app’s data folder.
- Tap **Train on device** → JNI calls `Trainer.train(datasetFolder)` in the Android library; it runs LiteRT training and saves updated weights file under app storage.
- Tap **Infer (trained)** → JNI calls `Trainer.infer(imagePath)`; it runs the updated model and returns a label/score.

## Notes
- For real training you must provide a **training-enabled `.tflite` model** (with exported train/infer/save/restore signatures) and wire its path in the Android library.  
- Keep datasets **small** and use **transfer learning**; full training on phone is impractical except for tiny models.  
- For background on the approach and limitations, see the references above.
