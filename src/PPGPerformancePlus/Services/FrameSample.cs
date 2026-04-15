namespace PPGPerformancePlus.Services;

public readonly record struct FrameSample(DateTimeOffset TimestampUtc, double FrameTimeMs);
