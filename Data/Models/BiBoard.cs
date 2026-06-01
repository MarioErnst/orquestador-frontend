namespace OrquestadorFrontend.Data.Models;

// A Power BI board entry shown in the Reports feature. Filtering by role is
// the repository's job; this model just describes the board.
public sealed record BiBoard(
    string Id,
    string Name,
    string Description,
    string? ThumbnailRef = null);
