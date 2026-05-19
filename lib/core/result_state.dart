// Models the four transversal states every data-bound screen must handle:
// Loading, Data, Empty, Failure. Sealed so callers must exhaustively switch.

sealed class ResultState<T> {
  const ResultState();
}

final class Loading<T> extends ResultState<T> {
  const Loading();
}

final class Data<T> extends ResultState<T> {
  final T value;
  const Data(this.value);
}

final class Empty<T> extends ResultState<T> {
  const Empty();
}

final class Failure<T> extends ResultState<T> {
  final String message;
  final Object? cause;
  const Failure(this.message, {this.cause});
}
