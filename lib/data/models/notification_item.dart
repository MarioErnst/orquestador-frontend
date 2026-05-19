// A notification entry shown in the Notifications feature and used by the
// home screen's "Alertas recientes" summary block.

enum NotificationKind {
  newBoard,
  indicatorThresholdExceeded,
  newReport,
  whistleblowerAlert,
}

class NotificationItem {
  final String id;
  final NotificationKind kind;
  final String title;
  final String body;
  final DateTime receivedAt;
  // Opaque target identifier (e.g., a report id, a board id, a case id).
  // The UI maps it to a navigation action.
  final String? targetRef;
  final bool read;

  const NotificationItem({
    required this.id,
    required this.kind,
    required this.title,
    required this.body,
    required this.receivedAt,
    this.targetRef,
    this.read = false,
  });
}
