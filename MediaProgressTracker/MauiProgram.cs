using MediaProgressTracker.Pages;
using MediaProgressTracker.Services.Abstract;
using MediaProgressTracker.ViewModels;
using Microsoft.Extensions.Logging;

namespace MediaProgressTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<ISteamSpyService, SteamSpyService>();

            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<MainViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
