using System.Collections.Concurrent;
namespace AI.Interviewer.Api.State;

public class InterviewConnectionManager
{
    private readonly ConcurrentDictionary<string, InterviewConnectionState> _connections = new();
    public void Add(string connectionId, InterviewConnectionState state)
    {
        _connections[connectionId] = state;
    }

    public InterviewConnectionState? Get(string connectionId)
    {
        return _connections.GetValueOrDefault(connectionId);
    }

    public void Remove(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }
}
