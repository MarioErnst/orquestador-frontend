// A document item shown in the Documents feature. Three concrete kinds map to
// the three real-world sources: a PDF report, an institutional video, and a
// SharePoint link inheriting permissions from the user identity.

enum ReportKind {
  pdf,
  video,
  sharepointLink,
}

class Report {
  final String id;
  final String title;
  final String description;
  final ReportKind kind;
  final DateTime publishedAt;
  // For PDFs this is also the source of the AI summary placeholder.
  final String? aiSummary;
  // Mock placeholder URL or asset reference; real URLs come from the backend.
  final String? sourceRef;

  const Report({
    required this.id,
    required this.title,
    required this.description,
    required this.kind,
    required this.publishedAt,
    this.aiSummary,
    this.sourceRef,
  });
}
