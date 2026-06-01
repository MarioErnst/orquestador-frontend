namespace OrquestadorFrontend.Data.Mock;

// Shared helper to make every mock repository feel like a real network call,
// so loading states are demoable and the UI is built against realistic timing.
internal static class MockLatency
{
    private static readonly Random _random = new();

    public static Task SimulateAsync()
    {
        var ms = 400 + _random.Next(400);
        return Task.Delay(ms);
    }
}
