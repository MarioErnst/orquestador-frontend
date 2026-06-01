using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Whistleblower;

public sealed partial class WhistleblowerListViewModel : ObservableObject
{
    private readonly IWhistleblowerRepository _whistleblower;
    private readonly ISessionService _session;

    [ObservableProperty]
    private ResultState<IReadOnlyList<WhistleblowerCase>> _state =
        new Loading<IReadOnlyList<WhistleblowerCase>>();

    public WhistleblowerListViewModel(IWhistleblowerRepository whistleblower, ISessionService session)
    {
        _whistleblower = whistleblower;
        _session = session;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        State = new Loading<IReadOnlyList<WhistleblowerCase>>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            // Production: audit event is written by the backend before this
            // call returns. The Shell guard ensures unauthorized roles never
            // reach this code path.
            var cases = await _whistleblower.GetCasesAsync(role);
            State = cases.Count == 0
                ? new Empty<IReadOnlyList<WhistleblowerCase>>()
                : new Data<IReadOnlyList<WhistleblowerCase>>(cases);
        }
        catch (WhistleblowerAccessDeniedException)
        {
            // Defense in depth: if a user arrived here without permission,
            // close immediately and never reveal data. Generic message only.
            State = new Failure<IReadOnlyList<WhistleblowerCase>>(
                "No tenés acceso a este módulo.");
        }
        catch (Exception)
        {
            State = new Failure<IReadOnlyList<WhistleblowerCase>>(
                "No pudimos cargar los casos. Intentá de nuevo.");
        }
    }

    [RelayCommand]
    private async Task OpenCaseAsync(WhistleblowerCase item)
    {
        if (item is null) return;
        await Shell.Current.GoToAsync(
            $"whistleblower/case?id={Uri.EscapeDataString(item.Id)}");
    }
}
