import '../../core/roles.dart';
import '../models/whistleblower_case.dart';

// Contract for the Whistleblower channel. Access is restricted by role and
// the rule lives in the data layer so every consumer (UI, router, providers)
// gets the same answer.
//
// Reinforced sensitivity (spec §5.6):
// - hasAccess returns false for roles without permission so the UI can avoid
//   rendering the entry at all (absence, not a disabled control).
// - getCases / getCase MUST throw WhistleblowerAccessDenied when called by a
//   role without access. Production will additionally record an audit event
//   before serving; the prototype only enforces the access rule.

abstract class WhistleblowerRepository {
  bool hasAccess(UserRole role);
  Future<List<WhistleblowerCase>> getCases(UserRole role);
  Future<WhistleblowerCase?> getCase(String id, UserRole role);
}

class WhistleblowerAccessDenied implements Exception {
  final UserRole role;
  WhistleblowerAccessDenied(this.role);
  @override
  String toString() =>
      'WhistleblowerAccessDenied: role ${role.name} is not allowed';
}
