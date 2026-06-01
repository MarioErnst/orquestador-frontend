using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockReportsRepository : IReportsRepository
{
    public bool SimulateError { get; set; }

    public async Task<IReadOnlyList<Report>> GetReportsAsync(UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load reports");
        return MockData.ReportsForRole(role);
    }

    public async Task<Report?> GetReportAsync(string id, UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load report");
        return MockData.ReportsForRole(role).FirstOrDefault(r => r.Id == id);
    }
}
