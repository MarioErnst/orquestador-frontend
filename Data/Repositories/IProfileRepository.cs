using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Profile feature. Receives the active role because the
// prototype lets the demo switch between roles after the mock login.
public interface IProfileRepository
{
    Task<UserProfile> GetProfileAsync(UserRole activeRole);
}
