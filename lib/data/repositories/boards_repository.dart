import '../../core/roles.dart';
import '../models/bi_board.dart';

// Contract for the Reports feature (Power BI boards).

abstract class BoardsRepository {
  Future<List<BiBoard>> getBoards(UserRole role);
  Future<BiBoard?> getBoard(String id, UserRole role);
}
