# AndroidTrainingLib (Kotlin, Android Studio)

This is a minimal **Android library module** exposing two static methods via JNI:

- `Trainer.train(datasetFolder: String)` – runs the model's **train** signature
- `Trainer.infer(imagePath: String)` – runs the model's **infer** signature

It depends on TensorFlow Lite (LiteRT) artifacts. Build this module in **Android Studio** (AGP + Gradle), then copy the generated **AAR** to the MAUI app and reference it in the `.csproj` via `<AndroidLibrary>`.

**Important:** your `.tflite` must be a **training-enabled** LiteRT model with exported **signatures**: `train`, `infer`, `save`, `restore`. See Google’s docs for details:
- On‑device training tutorial (LiteRT): https://ai.google.dev/edge/litert/conversion/tensorflow/build/ondevice_training
- TensorFlow blog: on‑device training in TensorFlow Lite: https://blog.tensorflow.org/2021/11/on-device-training-in-tensorflow-lite.html
