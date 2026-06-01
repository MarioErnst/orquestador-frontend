using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockBoardsRepository : IBoardsRepository
{
    // Flip to true to demo the ErrorState during the presentation.
    public bool SimulateError { get; set; }

    public async Task<IReadOnlyList<BiBoard>> GetBoardsAsync(UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load boards");
        return MockData.BoardsForRole(role);
    }

    public async Task<BiBoard?> GetBoardAsync(string id, UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load board");
        return MockData.BoardsForRole(role).FirstOrDefault(b => b.Id == id);
    }
}
