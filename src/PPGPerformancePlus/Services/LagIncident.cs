namespace PPGPerformancePlus.Services;

public sealed record LagIncident(
    DateTimeOffset TimestampUtc,
    string SuspectedSource,
    double FrameTimeMs,
    string Reason);
