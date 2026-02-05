using System.Runtime.InteropServices;
using Android.Runtime; // for JNI when on Android

namespace MauiOnDeviceTraining.Services;

public class Trainer : ITrainer
{
#if ANDROID
    const string JavaClass = "com.example.androidtraininglib.Trainer"; // Kotlin/Java class in AAR
#endif

    public Task<string> TrainAsync(string datasetFolder)
    {
#if ANDROID
        try
        {
            using var env = JNIEnv.ThreadEnv;
            var jcls = env.FindClass(JavaClass.Replace('.', '/'));
            var mid = env.GetStaticMethodID(jcls, "train", "(Ljava/lang/String;)Ljava/lang/String;");
            using var jpath = new Java.Lang.String(datasetFolder);
            var jret = (Java.Lang.String)env.CallStaticObjectMethod(jcls, mid, new JValue(jpath));
            return Task.FromResult(jret.ToString());
        }
        catch (Exception ex) { return Task.FromResult($"Training not available: {ex.Message}
Did you add the AAR and dependencies?"); }
#else
        return Task.FromResult("Training only supported on Android");
#endif
    }

    public Task<string> InferAsync(string imagePath)
    {
#if ANDROID
        try
        {
            using var env = JNIEnv.ThreadEnv;
            var jcls = env.FindClass(JavaClass.Replace('.', '/'));
            var mid = env.GetStaticMethodID(jcls, "infer", "(Ljava/lang/String;)Ljava/lang/String;");
            using var jpath = new Java.Lang.String(imagePath);
            var jret = (Java.Lang.String)env.CallStaticObjectMethod(jcls, mid, new JValue(jpath));
            return Task.FromResult(jret.ToString());
        }
        catch (Exception ex) { return Task.FromResult($"Inference not available: {ex.Message}
Did you add the AAR and dependencies?"); }
#else
        return Task.FromResult("Inference only supported on Android");
#endif
    }
}
