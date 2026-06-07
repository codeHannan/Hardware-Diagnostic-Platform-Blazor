namespace BenchRig.App.Core.Models.Compute;

/// <summary>Auto-detected (and user-supplemented) hardware information.</summary>
public record HardwareInfo
{
    public int Threads { get; init; }
    public double DeviceMemoryGb { get; init; }
    public string GpuVendor { get; init; } = "";
    public string GpuRenderer { get; init; } = "";
    public bool Webgl2 { get; init; }
    public bool Webgpu { get; init; }
    public int JsHeapLimitMb { get; init; }
    public string UserAgent { get; init; } = "";

    // User-selected (browsers don't expose the CPU model)
    public string CpuName { get; init; } = "";
    public string GpuName { get; init; } = "";
}

public record CpuBenchmarkResult
{
    public double SingleUps { get; init; }       // work-units/sec, 1 thread
    public double MultiUps { get; init; }         // work-units/sec, N threads
    public int ThreadsUsed { get; init; }
    public double DurationMs { get; init; }

    public double SingleScore { get; init; }      // normalized
    public double MultiScore { get; init; }
    public double EstGeekbenchSingle { get; init; }
    public double EstGeekbenchMulti { get; init; }

    // Legacy aliases used by the report
    public double Score => MultiScore;
    public double OpsPerSec => MultiUps;
    public double MultiThreadRatio => SingleUps > 0 ? MultiUps / SingleUps : 0;
}

public record GpuBenchmarkResult
{
    public double AvgFps { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Complexity { get; init; }
    public int Frames { get; init; }
    public bool Supported { get; init; } = true;
    public double Score { get; init; }
    public double VramEstimateMb { get; init; }

    public int ShaderComplexity => Complexity;
    public string Resolution => $"{Width}×{Height}";
}

public record MemoryBenchmarkResult
{
    public double PeakMb { get; init; }
    public bool Oom { get; init; }
    public double WriteGBs { get; init; }
    public double ReadGBs { get; init; }
    public int HeapLimitMb { get; init; }
    public double Score { get; init; }
}

public record BenchmarkSuite
{
    public CpuBenchmarkResult? Cpu { get; init; }
    public GpuBenchmarkResult? Gpu { get; init; }
    public MemoryBenchmarkResult? Memory { get; init; }
    public double CompositeScore => (Cpu?.MultiScore ?? 0) + (Gpu?.Score ?? 0) + (Memory?.Score ?? 0);
}

/// <summary>Progress payload pushed from a benchmark worker.</summary>
public record BenchmarkProgress
{
    public double Percent { get; init; }
    public double CurrentMetric { get; init; }
}
