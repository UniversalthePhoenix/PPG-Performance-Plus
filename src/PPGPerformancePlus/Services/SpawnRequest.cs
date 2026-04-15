namespace PPGPerformancePlus.Services;

public sealed class SpawnRequest
{
    public required string SourceId { get; init; }
    public required string DisplayName { get; init; }
    public required int EntityCount { get; init; }
    public required int JointCount { get; init; }
    public required Action<int> SpawnBatchAction { get; init; }

    public bool IsHeavy(double threshold) => EntityCount >= threshold || JointCount >= threshold;
}
