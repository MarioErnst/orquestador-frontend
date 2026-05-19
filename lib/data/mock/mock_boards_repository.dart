import '../../core/roles.dart';
import '../models/bi_board.dart';
import '../repositories/boards_repository.dart';
import '_latency.dart';
import 'mock_data.dart';

class MockBoardsRepository implements BoardsRepository {
  bool simulateError = false;

  @override
  Future<List<BiBoard>> getBoards(UserRole role) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load boards');
    }
    return MockData.boardsForRole(role);
  }

  @override
  Future<BiBoard?> getBoard(String id, UserRole role) async {
    await simulateLatency();
    if (simulateError) {
      throw Exception('Mock: failed to load board');
    }
    final boards = MockData.boardsForRole(role);
    for (final b in boards) {
      if (b.id == id) return b;
    }
    return null;
  }
}
