using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Reports;

[QueryProperty(nameof(BoardId), "id")]
public sealed partial class BoardViewerViewModel : ObservableObject
{
    private readonly IBoardsRepository _boards;
    private readonly ISessionService _session;

    [ObservableProperty]
    private string _boardId = string.Empty;

    [ObservableProperty]
    private ResultState<BiBoard> _state = new Loading<BiBoard>();

    public BoardViewerViewModel(IBoardsRepository boards, ISessionService session)
    {
        _boards = boards;
        _session = session;
    }

    partial void OnBoardIdChanged(string value)
    {
        // Each time Shell rebinds the BoardId query parameter we reload.
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(BoardId)) return;

        State = new Loading<BiBoard>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var board = await _boards.GetBoardAsync(BoardId, role);
            State = board is null
                ? new Empty<BiBoard>()
                : new Data<BiBoard>(board);
        }
        catch (Exception)
        {
            State = new Failure<BiBoard>(
                "No pudimos cargar el tablero. Intentá de nuevo.");
        }
    }
}
