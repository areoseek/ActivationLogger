using InputKit.Shared.Controls;
using Mopups.Hosting;
using UraniumUI;
using UraniumUI.Icons.MaterialSymbols;

namespace ActivationLoggerAlpha
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseUraniumUIBlurs()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddMaterialSymbolsFonts();

                });

            builder.Services.AddMopupsDialogs();
            return builder.Build();
        }
    }
}