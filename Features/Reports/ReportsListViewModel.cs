using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Reports;

public sealed partial class ReportsListViewModel : ObservableObject
{
    private readonly IBoardsRepository _boards;
    private readonly ISessionService _session;

    [ObservableProperty]
    private ResultState<IReadOnlyList<BiBoard>> _state =
        new Loading<IReadOnlyList<BiBoard>>();

    public ReportsListViewModel(IBoardsRepository boards, ISessionService session)
    {
        _boards = boards;
        _session = session;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        State = new Loading<IReadOnlyList<BiBoard>>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var boards = await _boards.GetBoardsAsync(role);
            State = boards.Count == 0
                ? new Empty<IReadOnlyList<BiBoard>>()
                : new Data<IReadOnlyList<BiBoard>>(boards);
        }
        catch (Exception)
        {
            State = new Failure<IReadOnlyList<BiBoard>>(
                "No pudimos cargar los tableros. Intentá de nuevo.");
        }
    }

    [ObservableProperty]
    private bool _isRefreshing;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var boards = await _boards.GetBoardsAsync(role);
            State = boards.Count == 0
                ? new Empty<IReadOnlyList<BiBoard>>()
                : new Data<IReadOnlyList<BiBoard>>(boards);
        }
        catch (Exception)
        {
            // Silent: existing data stays visible, spinner stops.
            // Full error path is shown only on initial Load.
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task OpenBoardAsync(BiBoard board)
    {
        if (board is null) return;
        await Shell.Current.GoToAsync($"board?id={Uri.EscapeDataString(board.Id)}");
    }
}
