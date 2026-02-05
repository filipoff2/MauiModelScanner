using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using MauiOnDeviceTraining.Services;

namespace MauiOnDeviceTraining;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCamera()
            .UseSkiaSharp(true)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<ITrainer, Trainer>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
