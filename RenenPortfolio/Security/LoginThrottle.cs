using System.Collections.Concurrent;

namespace RenenPortfolio.Security;

public interface ILoginThrottle
{
    bool IsLockedOut(string key, out TimeSpan retryAfter);
    void RecordFailure(string key);
    void Reset(string key);
}
public class LoginThrottle : ILoginThrottle
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, (int Count, DateTimeOffset First)> _attempts = new();

    public bool IsLockedOut(string key, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;

        if (!_attempts.TryGetValue(key, out var entry))
        {
            return false;
        }

        var elapsed = DateTimeOffset.UtcNow - entry.First;
        if (elapsed > Window)
        {
            _attempts.TryRemove(key, out _);
            return false;
        }

        if (entry.Count < MaxAttempts)
        {
            return false;
        }

        retryAfter = Window - elapsed;
        return true;
    }

    public void RecordFailure(string key) =>
        _attempts.AddOrUpdate(
            key,
            _ => (1, DateTimeOffset.UtcNow),
            (_, existing) => DateTimeOffset.UtcNow - existing.First > Window
                ? (1, DateTimeOffset.UtcNow)
                : (existing.Count + 1, existing.First));

    public void Reset(string key) => _attempts.TryRemove(key, out _);
}