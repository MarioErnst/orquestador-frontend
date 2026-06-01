using Microsoft.Extensions.Logging;
using OrquestadorFrontend.Data.Mock;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

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

        // --- Data layer: today mocks, tomorrow API implementations of the
        // same interfaces. Repositories are Singleton because the mocks keep
        // in-memory state (read notifications, preferences) that must outlive
        // a single page navigation.
        builder.Services.AddSingleton<IBoardsRepository, MockBoardsRepository>();
        builder.Services.AddSingleton<IDashboardRepository, MockDashboardRepository>();
        builder.Services.AddSingleton<INotificationsRepository, MockNotificationsRepository>();
        builder.Services.AddSingleton<IProfileRepository, MockProfileRepository>();
        builder.Services.AddSingleton<IReportsRepository, MockReportsRepository>();
        builder.Services.AddSingleton<IWhistleblowerRepository, MockWhistleblowerRepository>();

        // --- Services: session is Singleton because the Shell guard and any
        // ViewModel must read the same active role; notification routing is
        // stateless but registered Singleton for consistency.
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<INotificationNavigationService, NotificationNavigationService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
