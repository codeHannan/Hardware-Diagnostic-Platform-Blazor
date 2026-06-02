namespace BenchRig.App.Core.Models.Compute;

public record CpuBenchmarkResult
{
    public double Score { get; init; }
    public int ThreadsUsed { get; init; }
    public double DurationMs { get; init; }
    public double OpsPerSec { get; init; }
}

public record GpuBenchmarkResult
{
    public double Score { get; init; }
    public double AvgFps { get; init; }
    public int ShaderComplexity { get; init; }
    public double VramEstimateMb { get; init; }
}

public record BenchmarkSuite
{
    public CpuBenchmarkResult? Cpu { get; init; }
    public GpuBenchmarkResult? Gpu { get; init; }
    public double CompositeScore => (Cpu?.Score ?? 0) + (Gpu?.Score ?? 0);
}

/// <summary>Progress payload pushed from a benchmark worker.</summary>
public record BenchmarkProgress
{
    public double Percent { get; init; }
    public double CurrentMetric { get; init; }
}
