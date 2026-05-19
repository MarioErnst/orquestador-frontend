import '../../core/roles.dart';
import '../models/whistleblower_case.dart';
import '../repositories/whistleblower_repository.dart';
import '_latency.dart';
import 'mock_data.dart';

class MockWhistleblowerRepository implements WhistleblowerRepository {
  bool simulateError = false;

  @override
  bool hasAccess(UserRole role) =>
      MockData.whistleblowerAllowedRoles.contains(role);

  @override
  Future<List<WhistleblowerCase>> getCases(UserRole role) async {
    if (!hasAccess(role)) {
      throw WhistleblowerAccessDenied(role);
    }
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load whistleblower cases');
    }
    return MockData.whistleblowerCases();
  }

  @override
  Future<WhistleblowerCase?> getCase(String id, UserRole role) async {
    if (!hasAccess(role)) {
      throw WhistleblowerAccessDenied(role);
    }
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load whistleblower case');
    }
    for (final c in MockData.whistleblowerCases()) {
      if (c.id == id) return c;
    }
    return null;
  }
}
