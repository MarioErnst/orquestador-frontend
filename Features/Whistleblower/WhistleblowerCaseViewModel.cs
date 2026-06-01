using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Whistleblower;

[QueryProperty(nameof(CaseId), "id")]
public sealed partial class WhistleblowerCaseViewModel : ObservableObject
{
    private readonly IWhistleblowerRepository _whistleblower;
    private readonly ISessionService _session;

    [ObservableProperty] private string _caseId = string.Empty;
    [ObservableProperty] private ResultState<WhistleblowerCase> _state = new Loading<WhistleblowerCase>();
    [ObservableProperty] private WhistleblowerCase? _case;
    [ObservableProperty] private string _statusLabel = string.Empty;

    public WhistleblowerCaseViewModel(IWhistleblowerRepository whistleblower, ISessionService session)
    {
        _whistleblower = whistleblower;
        _session = session;
    }

    partial void OnCaseIdChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(CaseId)) return;

        State = new Loading<WhistleblowerCase>();
        Case = null;
        StatusLabel = string.Empty;

        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var item = await _whistleblower.GetCaseAsync(CaseId, role);
            if (item is null)
            {
                State = new Empty<WhistleblowerCase>();
                return;
            }
            Case = item;
            StatusLabel = WhistleblowerStatusLabelConverter.Label(item.Status);
            State = new Data<WhistleblowerCase>(item);
        }
        catch (WhistleblowerAccessDeniedException)
        {
            State = new Failure<WhistleblowerCase>("No tenés acceso a este módulo.");
        }
        catch (Exception)
        {
            State = new Failure<WhistleblowerCase>(
                "No pudimos cargar el caso. Intentá de nuevo.");
        }
    }
}
