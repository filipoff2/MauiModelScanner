
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiModelScanner;

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

        builder.Services.AddSingleton<Services.OnnxService>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
