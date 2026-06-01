using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Reports feature (Power BI boards).
public interface IBoardsRepository
{
    Task<IReadOnlyList<BiBoard>> GetBoardsAsync(UserRole role);
    Task<BiBoard?> GetBoardAsync(string id, UserRole role);
}
