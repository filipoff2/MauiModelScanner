
using CommunityToolkit.Maui;
using Microsoft.Maui;

namespace MauiModelScanner;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
    }
}
