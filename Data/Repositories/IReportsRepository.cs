using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Documents feature. The prototype's MockReportsRepository
// implements this; the production ApiReportsRepository will too, without
// touching the UI.
public interface IReportsRepository
{
    Task<IReadOnlyList<Report>> GetReportsAsync(UserRole role);
    Task<Report?> GetReportAsync(string id, UserRole role);
}
