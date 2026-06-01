using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockWhistleblowerRepository : IWhistleblowerRepository
{
    public bool SimulateError { get; set; }

    public bool HasAccess(UserRole role)
        => MockData.WhistleblowerAllowedRoles.Contains(role);

    public async Task<IReadOnlyList<WhistleblowerCase>> GetCasesAsync(UserRole role)
    {
        // Access check happens before any latency or error simulation so the
        // role gate is enforced even when the demo flips SimulateError on.
        if (!HasAccess(role)) throw new WhistleblowerAccessDeniedException(role);

        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load whistleblower cases");
        return MockData.WhistleblowerCases();
    }

    public async Task<WhistleblowerCase?> GetCaseAsync(string id, UserRole role)
    {
        if (!HasAccess(role)) throw new WhistleblowerAccessDeniedException(role);

        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load whistleblower case");
        return MockData.WhistleblowerCases().FirstOrDefault(c => c.Id == id);
    }
}
