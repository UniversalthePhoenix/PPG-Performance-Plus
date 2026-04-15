namespace PPGPerformancePlus.Services;

public sealed class SpawnQueue
{
    private readonly Queue<SpawnRequest> _queue = new();

    public int PendingCount => _queue.Count;
    public SpawnRequest? Current { get; private set; }

    public void Enqueue(SpawnRequest request) => _queue.Enqueue(request);

    public SpawnRequest? DequeueNext()
    {
        Current = _queue.Count > 0 ? _queue.Dequeue() : null;
        return Current;
    }

    public void Clear()
    {
        _queue.Clear();
        Current = null;
    }
}
