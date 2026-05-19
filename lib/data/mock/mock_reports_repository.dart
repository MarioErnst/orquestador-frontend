import '../../core/roles.dart';
import '../models/report.dart';
import '../repositories/reports_repository.dart';
import '_latency.dart';
import 'mock_data.dart';

class MockReportsRepository implements ReportsRepository {
  // Flip to true to demo the error_state during the presentation.
  bool simulateError = false;

  @override
  Future<List<Report>> getReports(UserRole role) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load reports');
    }
    return MockData.reportsForRole(role);
  }

  @override
  Future<Report?> getReport(String id, UserRole role) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load report');
    }
    final reports = MockData.reportsForRole(role);
    for (final r in reports) {
      if (r.id == id) return r;
    }
    return null;
  }
}
