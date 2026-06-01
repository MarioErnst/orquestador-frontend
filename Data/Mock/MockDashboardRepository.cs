using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockDashboardRepository : IDashboardRepository
{
    public bool SimulateError { get; set; }

    public async Task<IReadOnlyList<DashboardKpi>> GetKpisAsync(UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load KPIs");
        return MockData.KpisForRole(role);
    }
}
