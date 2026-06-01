using Microsoft.Extensions.Logging;

namespace OrquestadorFrontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // ACHS Masterbrand Variable Fonts. One file per family covers
                // every weight; FontWeight on a Label/Style picks the variation.
                fonts.AddFont("ACHSNuevaSans-VF.ttf", "ACHSNuevaSans");
                fonts.AddFont("ACHSNuevaSerif-VF.ttf", "ACHSNuevaSerif");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
