namespace OrquestadorFrontend.Data.Models;

public enum ReportKind
{
    Pdf,
    Video,
    SharepointLink
}

// A document item shown in the Documents feature. The three kinds map to the
// real-world sources: PDF reports, institutional videos and SharePoint links
// that inherit permissions from the user identity.
public sealed record Report(
    string Id,
    string Title,
    string Description,
    ReportKind Kind,
    DateTime PublishedAt,
    string? AiSummary = null,
    string? SourceRef = null);
