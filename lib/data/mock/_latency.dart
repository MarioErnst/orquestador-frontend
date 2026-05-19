import 'dart:math';

// Shared helper to make every mock repository feel like a real network call,
// so loading states are demoable and the UI is built against realistic timing.

final _random = Random();

Future<void> simulateLatency() {
  final ms = 400 + _random.nextInt(400);
  return Future.delayed(Duration(milliseconds: ms));
}
