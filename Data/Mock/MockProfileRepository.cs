using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockProfileRepository : IProfileRepository
{
    public bool SimulateError { get; set; }

    public async Task<UserProfile> GetProfileAsync(UserRole activeRole)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load profile");
        return MockData.ProfileForRole(activeRole);
    }
}
