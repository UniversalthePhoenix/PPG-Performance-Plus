namespace PPGPerformancePlus.Services;

public sealed class FrameTimeTracker
{
    private readonly Queue<FrameSample> _samples = new();
    private readonly int _maxSamples;

    public FrameTimeTracker(int maxSamples = 300)
    {
        _maxSamples = Math.Max(30, maxSamples);
    }

    public IReadOnlyCollection<FrameSample> Samples => _samples;
    public double LatestFrameMs { get; private set; }
    public double AverageFrameMs => _samples.Count == 0 ? 0d : _samples.Average(sample => sample.FrameTimeMs);
    public double WorstFrameMs => _samples.Count == 0 ? 0d : _samples.Max(sample => sample.FrameTimeMs);

    public void Record(double frameTimeMs)
    {
        LatestFrameMs = frameTimeMs;
        _samples.Enqueue(new FrameSample(DateTimeOffset.UtcNow, frameTimeMs));

        while (_samples.Count > _maxSamples)
        {
            _samples.Dequeue();
        }
    }
}
