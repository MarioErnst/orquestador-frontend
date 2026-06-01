namespace OrquestadorFrontend.Core;

// Discriminated union for screens that load data asynchronously. Each loading
// step transitions Loading -> (Data | Empty | Failure). Pages branch on the
// concrete record type via the converters in ResultStateConverters.cs.
public abstract record ResultState<T>;

public sealed record Loading<T>() : ResultState<T>;
public sealed record Data<T>(T Value) : ResultState<T>;
public sealed record Empty<T>() : ResultState<T>;
public sealed record Failure<T>(string Message) : ResultState<T>;
