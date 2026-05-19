// A whistleblower channel case. Treated with reinforced sensitivity in every
// layer: visibility, audit, and absence-for-unauthorized-roles. The case body
// content is intentionally optional and pending product definition.

enum WhistleblowerStatus {
  received,
  inInvestigation,
  closed,
}

class WhistleblowerCase {
  final String id;
  final String referenceCode;
  final WhistleblowerStatus status;
  final String category;
  final DateTime receivedAt;
  // Body content is gated behind a product decision (compliance area scope).
  // Until that is defined, the detail screen renders metadata only.
  final String? bodyPlaceholder;

  const WhistleblowerCase({
    required this.id,
    required this.referenceCode,
    required this.status,
    required this.category,
    required this.receivedAt,
    this.bodyPlaceholder,
  });
}
