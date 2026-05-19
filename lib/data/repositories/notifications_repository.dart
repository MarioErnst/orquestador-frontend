import '../../core/roles.dart';
import '../models/notification_item.dart';

// Contract for the Notifications feature. Preferences live here too because
// the same channel decides what arrives and what the user can mute.

abstract class NotificationsRepository {
  Future<List<NotificationItem>> getNotifications(UserRole role);

  // Per-kind opt-in/opt-out. Defaults to all enabled. Prototype keeps
  // state in memory; the future ApiNotificationsRepository will persist it.
  Future<Map<NotificationKind, bool>> getPreferences();
  Future<void> setPreference(NotificationKind kind, bool enabled);
}
