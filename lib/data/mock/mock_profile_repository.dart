import '../../core/roles.dart';
import '../models/user_profile.dart';
import '../repositories/profile_repository.dart';
import '_latency.dart';
import 'mock_data.dart';

class MockProfileRepository implements ProfileRepository {
  bool simulateError = false;

  @override
  Future<UserProfile> getProfile(UserRole activeRole) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load profile');
    }
    return MockData.profileForRole(activeRole);
  }
}
