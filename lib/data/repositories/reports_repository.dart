import '../../core/roles.dart';
import '../models/report.dart';

// Contract for the Documents feature. The prototype's MockReportsRepository
// implements this; the production ApiReportsRepository will too, without
// touching the UI.

abstract class ReportsRepository {
  Future<List<Report>> getReports(UserRole role);
  Future<Report?> getReport(String id, UserRole role);
}
