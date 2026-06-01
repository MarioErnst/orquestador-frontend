namespace OrquestadorFrontend.Data.Models;

public enum WhistleblowerStatus
{
    Received,
    InInvestigation,
    Closed
}

// A whistleblower channel case. Treated with reinforced sensitivity in every
// layer: visibility, audit, and absence-for-unauthorized-roles. The case body
// content is intentionally optional and pending product definition.
public sealed record WhistleblowerCase(
    string Id,
    string ReferenceCode,
    WhistleblowerStatus Status,
    string Category,
    DateTime ReceivedAt,
    string? BodyPlaceholder = null);
