import '../../core/roles.dart';

// The signed-in user as the app sees it. In production this is hydrated from
// Microsoft Entra ID; in the prototype it comes from the profile mock.

class UserProfile {
  final String id;
  final String fullName;
  final String email;
  final String position;
  final UserRole role;

  const UserProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.position,
    required this.role,
  });
}
