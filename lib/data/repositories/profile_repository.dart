import '../../core/roles.dart';
import '../models/user_profile.dart';

// Contract for the Profile feature. Receives the active role because the
// prototype lets the demo switch between roles after the mock login.

abstract class ProfileRepository {
  Future<UserProfile> getProfile(UserRole activeRole);
}
