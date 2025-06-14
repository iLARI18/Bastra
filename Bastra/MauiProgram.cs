using Microsoft.Extensions.Logging;
using Bastra.Models;
using Bastra.ModelsLogic;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio; 



namespace Bastra
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .AddAudio()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Game-Of-Squids.ttf", "MainFont");
                    fonts.AddFont("Cards.ttf", "CardsFont");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<Game>();
            builder.Services.AddTransient<INotificationManagerService, Bastra.Platforms.Android.NotificationManagerService>();
#endif
            return builder.Build();
        }
    }
}
