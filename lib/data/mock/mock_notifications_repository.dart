import '../../core/roles.dart';
import '../models/notification_item.dart';
import '../repositories/notifications_repository.dart';
import '_latency.dart';
import 'mock_data.dart';

class MockNotificationsRepository implements NotificationsRepository {
  bool simulateError = false;

  // In-memory preferences for the prototype. Default: all kinds enabled.
  final Map<NotificationKind, bool> _preferences = {
    for (final k in NotificationKind.values) k: true,
  };

  @override
  Future<List<NotificationItem>> getNotifications(UserRole role) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load notifications');
    }
    return MockData.notificationsForRole(role)
        .where((n) => _preferences[n.kind] ?? true)
        .toList();
  }

  @override
  Future<Map<NotificationKind, bool>> getPreferences() async {
    await simulateLatency();
    return Map.unmodifiable(_preferences);
  }

  @override
  Future<void> setPreference(NotificationKind kind, bool enabled) async {
    await simulateLatency();
    _preferences[kind] = enabled;
  }
}
