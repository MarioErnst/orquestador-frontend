import 'package:flutter_test/flutter_test.dart';
import 'package:orquestador_frontend/app/app.dart';

void main() {
  testWidgets('App boots and renders the placeholder title', (tester) async {
    await tester.pumpWidget(const App());
    expect(find.text('App para Directivos'), findsOneWidget);
  });
}
