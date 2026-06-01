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
                // Brand fonts are registered in a follow-up commit when the assets land.
                // Placeholder system fonts keep the app renderable in this scaffold.
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
