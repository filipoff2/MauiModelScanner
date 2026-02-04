
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Text;

namespace MauiModelScanner.Services;

public sealed class OnnxService : IDisposable
{
    private readonly Dictionary<string, InferenceSession> _sessions = new();

    public async Task<string> PredictAsync(Stream imageStream, string modelPath)
    {
        var session = GetOrCreateSession(modelPath);

        // Preprocess to 224x224 RGB float32 CHW with ImageNet normalization
        var inputMeta = session.InputMetadata.First();
        var inputName = inputMeta.Key;
        var (tensor, w, h) = await Task.Run(() => PreprocessToCHW(imageStream, 224, 224));

        using var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, tensor)
        };

        using var results = session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();
        var probs = Softmax(output);
        int argmax = Array.IndexOf(probs, probs.Max());
        var topP = probs[argmax];

        // Labels: try <model>.labels.txt or labels.txt in same folder
        string label = $"Class #{argmax}";
        var labelsPath = Path.ChangeExtension(modelPath, ".labels.txt");
        if (!File.Exists(labelsPath))
        {
            var dir = Path.GetDirectoryName(modelPath)!;
            var alt = Path.Combine(dir, "labels.txt");
            if (File.Exists(alt)) labelsPath = alt;
        }
        if (File.Exists(labelsPath))
        {
            var labels = await File.ReadAllLinesAsync(labelsPath);
            if (argmax >= 0 && argmax < labels.Length) label = labels[argmax];
        }

        return $"{label}: {topP:P1}";
    }

    private InferenceSession GetOrCreateSession(string modelPath)
    {
        if (_sessions.TryGetValue(modelPath, out var existing))
            return existing;

        var so = new SessionOptions();
        // NOTE: On Android, default CPU EP works. For NNAPI, a custom build or Java bindings may be required.
        // so.AppendExecutionProvider_Nnapi(); // exposed in native/Java; C# availability varies by package.
        var session = new InferenceSession(modelPath, so);
        _sessions[modelPath] = session;
        return session;
    }

    private static (DenseTensor<float> tensor, int w, int h) PreprocessToCHW(Stream input, int width, int height)
    {
        using var data = SKData.Create(input);
        using var img = SKImage.FromEncodedData(data);
        using var resized = img.Resize(new SKImageInfo(width, height, SKColorType.Rgba8888), SKFilterQuality.Medium);
        using var bmp = SKBitmap.FromImage(resized);
        var pixels = bmp.Pixels; // RGBA

        var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });
        float[] mean = { 0.485f, 0.456f, 0.406f };
        float[] std  = { 0.229f, 0.224f, 0.225f };

        int idx = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, idx++)
            {
                var c = pixels[idx];
                float r = c.Red / 255f;
                float g = c.Green / 255f;
                float b = c.Blue / 255f;
                tensor[0, 0, y, x] = (r - mean[0]) / std[0];
                tensor[0, 1, y, x] = (g - mean[1]) / std[1];
                tensor[0, 2, y, x] = (b - mean[2]) / std[2];
            }
        }
        return (tensor, width, height);
    }

    private static float[] Softmax(float[] logits)
    {
        var max = logits.Max();
        var exps = logits.Select(v => MathF.Exp(v - max)).ToArray();
        var sum = exps.Sum();
        for (int i = 0; i < exps.Length; i++) exps[i] /= sum;
        return exps;
    }

    public void Dispose()
    {
        foreach (var s in _sessions.Values) s.Dispose();
        _sessions.Clear();
    }
}
