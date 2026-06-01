using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Whistleblower channel. Access is restricted by role and
// the rule lives in the data layer so every consumer (UI, Shell guard,
// ViewModels) gets the same answer.
//
// Reinforced sensitivity (Propuesta 1 section 8, Propuesta 2 section 4.6):
// - HasAccess returns false for roles without permission so the UI can avoid
//   rendering the entry at all (absence, not a disabled control).
// - GetCasesAsync / GetCaseAsync MUST throw WhistleblowerAccessDeniedException
//   when called by a role without access. Production will additionally record
//   an audit event before serving; the prototype only enforces the access rule.
public interface IWhistleblowerRepository
{
    bool HasAccess(UserRole role);
    Task<IReadOnlyList<WhistleblowerCase>> GetCasesAsync(UserRole role);
    Task<WhistleblowerCase?> GetCaseAsync(string id, UserRole role);
}

public sealed class WhistleblowerAccessDeniedException : Exception
{
    public UserRole Role { get; }

    public WhistleblowerAccessDeniedException(UserRole role)
        : base($"Role {role} is not allowed in the Whistleblower channel")
    {
        Role = role;
    }
}
