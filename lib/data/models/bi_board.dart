// A Power BI board entry shown in the Reports feature. Filtering by role is
// the repository's job; this model just describes the board.

class BiBoard {
  final String id;
  final String name;
  final String description;
  // Optional pictogram or thumbnail asset reference for the list view.
  final String? thumbnailRef;

  const BiBoard({
    required this.id,
    required this.name,
    required this.description,
    this.thumbnailRef,
  });
}
