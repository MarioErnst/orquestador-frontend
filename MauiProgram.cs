using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using OrquestadorFrontend.Data.Mock;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Features.Auth;
using OrquestadorFrontend.Features.Documents;
using OrquestadorFrontend.Features.Home;
using OrquestadorFrontend.Features.Notifications;
using OrquestadorFrontend.Features.Profile;
using OrquestadorFrontend.Features.Reports;
using OrquestadorFrontend.Features.Whistleblower;
using OrquestadorFrontend.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace OrquestadorFrontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            // CommunityToolkit.Maui — TouchBehavior, AnimationBehavior,
            // Popup, StatusBarBehavior. Required initialiser per the
            // package analyzer (MCT001).
            .UseMauiCommunityToolkit()
            // SkiaSharp views for hero visuals (gradients, skeleton
            // loaders, custom charts).
            .UseSkiaSharp()
            // UraniumUI Material — Material 3 form controls with floating
            // labels and proper focus/error states. Theme tokens are still
            // owned by our own Colors.xaml; UraniumUI only provides the
            // control templates.
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                // ACHS Masterbrand Variable Fonts. One file per family covers
                // every weight; FontWeight on a Label/Style picks the variation.
                fonts.AddFont("ACHSNuevaSans-VF.ttf", "ACHSNuevaSans");
                fonts.AddFont("ACHSNuevaSerif-VF.ttf", "ACHSNuevaSerif");

                // Icon font: Google Material Icons v4.0.0 (Apache 2.0).
                // Referenced in XAML via FontFamily="MaterialIcons" and the
                // glyph codepoints from the public ligature/codepoints table.
                // UraniumUI controls also consume this family internally.
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // --- Shell + App
        builder.Services.AddSingleton<AppShell>();

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

        // --- Services
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<INotificationNavigationService, NotificationNavigationService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();

        // --- Pages and ViewModels: Transient so every navigation gets a fresh
        // instance bound to a fresh ViewModel.
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<BiometricReauthPage>();
        builder.Services.AddTransient<BiometricReauthViewModel>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HomeViewModel>();

        builder.Services.AddTransient<ReportsListPage>();
        builder.Services.AddTransient<ReportsListViewModel>();
        builder.Services.AddTransient<BoardViewerPage>();
        builder.Services.AddTransient<BoardViewerViewModel>();

        builder.Services.AddTransient<DocumentsListPage>();
        builder.Services.AddTransient<DocumentsListViewModel>();
        builder.Services.AddTransient<DocumentViewerPage>();
        builder.Services.AddTransient<DocumentViewerViewModel>();

        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<NotificationPreferencesPage>();
        builder.Services.AddTransient<NotificationPreferencesViewModel>();

        builder.Services.AddTransient<WhistleblowerListPage>();
        builder.Services.AddTransient<WhistleblowerListViewModel>();
        builder.Services.AddTransient<WhistleblowerCasePage>();
        builder.Services.AddTransient<WhistleblowerCaseViewModel>();

        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfileViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
