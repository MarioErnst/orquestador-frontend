using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Documents;

[QueryProperty(nameof(ReportId), "id")]
public sealed partial class DocumentViewerViewModel : ObservableObject
{
    private readonly IReportsRepository _reports;
    private readonly ISessionService _session;

    [ObservableProperty] private string _reportId = string.Empty;
    [ObservableProperty] private ResultState<Report> _state = new Loading<Report>();
    [ObservableProperty] private Report? _report;
    [ObservableProperty] private bool _isPdf;
    [ObservableProperty] private bool _isVideo;
    [ObservableProperty] private bool _isSharepoint;
    [ObservableProperty] private bool _hasAiSummary;

    public DocumentViewerViewModel(IReportsRepository reports, ISessionService session)
    {
        _reports = reports;
        _session = session;
    }

    partial void OnReportIdChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(ReportId)) return;

        State = new Loading<Report>();
        Report = null;
        IsPdf = IsVideo = IsSharepoint = HasAiSummary = false;

        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var report = await _reports.GetReportAsync(ReportId, role);
            if (report is null)
            {
                State = new Empty<Report>();
                return;
            }

            Report = report;
            IsPdf = report.Kind == ReportKind.Pdf;
            IsVideo = report.Kind == ReportKind.Video;
            IsSharepoint = report.Kind == ReportKind.SharepointLink;
            HasAiSummary = !string.IsNullOrEmpty(report.AiSummary);
            State = new Data<Report>(report);
        }
        catch (Exception)
        {
            State = new Failure<Report>(
                "No pudimos cargar el documento. Intentá de nuevo.");
        }
    }
}
