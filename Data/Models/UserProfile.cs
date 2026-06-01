using OrquestadorFrontend.Core;

namespace OrquestadorFrontend.Data.Models;

// The signed-in user as the app sees it. In production this is hydrated from
// Microsoft Entra ID; in the prototype it comes from the profile mock.
public sealed record UserProfile(
    string Id,
    string FullName,
    string Email,
    string Position,
    UserRole Role);
