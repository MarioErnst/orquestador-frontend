using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Documents;

public sealed partial class DocumentsListViewModel : ObservableObject
{
    private readonly IReportsRepository _reports;
    private readonly ISessionService _session;

    [ObservableProperty]
    private ResultState<IReadOnlyList<Report>> _state =
        new Loading<IReadOnlyList<Report>>();

    public DocumentsListViewModel(IReportsRepository reports, ISessionService session)
    {
        _reports = reports;
        _session = session;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        State = new Loading<IReadOnlyList<Report>>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var reports = await _reports.GetReportsAsync(role);
            State = reports.Count == 0
                ? new Empty<IReadOnlyList<Report>>()
                : new Data<IReadOnlyList<Report>>(reports);
        }
        catch (Exception)
        {
            State = new Failure<IReadOnlyList<Report>>(
                "No pudimos cargar los documentos. Intentá de nuevo.");
        }
    }

    [RelayCommand]
    private async Task OpenDocumentAsync(Report report)
    {
        if (report is null) return;
        await Shell.Current.GoToAsync($"document?id={Uri.EscapeDataString(report.Id)}");
    }

    public static string KindLabel(ReportKind kind) => kind switch
    {
        ReportKind.Pdf => "PDF",
        ReportKind.Video => "Video",
        ReportKind.SharepointLink => "SharePoint",
        _ => string.Empty
    };
}
