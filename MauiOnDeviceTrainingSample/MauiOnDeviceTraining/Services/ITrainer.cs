namespace MauiOnDeviceTraining.Services;

public interface ITrainer
{
    Task<string> TrainAsync(string datasetFolder);
    Task<string> InferAsync(string imagePath);
}
