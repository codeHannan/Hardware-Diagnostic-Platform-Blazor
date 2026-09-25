namespace BenchRig.App.Core.Constants;

public record CpuPreset(string Name, int DurationMsPerPass, string Desc);
public record GpuPreset(string Name, int Width, int Height, int Complexity, int DurationMs, string Desc);
public record MemoryPreset(string Name, int TargetMb, string Desc);
public record ReferenceCpu(string Name, double Single, double Multi);
public record ReferenceGpu(string Name, double Score);

/// <summary>
/// Benchmark presets + approximate reference scores for cross-platform context.
/// Reference numbers are scaled to mirror public Geekbench-6 / relative-GPU ratios;
/// they are approximations for comparison, not certified results.
/// </summary>
public static class ComputePresets
{
    public static readonly CpuPreset[] Cpu =
    [
        new("Quick", 5_000, "5s per pass — fast estimate"),
        new("Standard", 10_000, "10s per pass — balanced"),
        new("Extended", 20_000, "20s per pass — steady-state"),
        new("Stress", 30_000, "30s per pass — sustained load"),
    ];

    // Complexity = volumetric ray-march steps (each step = 5-octave 3D fbm, very heavy).
    public static readonly GpuPreset[] Gpu =
    [
        new("720p · Light", 1280, 720, 64, 8_000, "Entry volumetric ray-march at 720p"),
        new("1080p · Medium", 1920, 1080, 96, 10_000, "Standard 1080p volumetric load"),
        new("1440p · Heavy", 2560, 1440, 128, 12_000, "Demanding 1440p volumetric load"),
        new("4K · Extreme", 3840, 2160, 160, 12_000, "Maximum-stress 4K volumetric load"),
    ];

    public static readonly MemoryPreset[] Memory =
    [
        new("512 MB", 512, "Quick allocation + bandwidth"),
        new("1 GB", 1024, "Standard"),
        new("2 GB", 2048, "Heavy"),
        new("4 GB", 4096, "Overload (may hit the tab heap limit)"),
    ];

    // Normalization: a reference mid-range core ≈ 1000 single. Multi scales with cores.
    public static readonly ReferenceCpu[] ReferenceCpus =
    [
        new("Intel Core i5-8250U", 700, 2200),
        new("Apple M1", 2300, 8300),
        new("AMD Ryzen 5 5600X", 1500, 9000),
        new("Intel Core i7-12700K", 2000, 14000),
        new("Apple M2 Pro", 2600, 14000),
        new("AMD Ryzen 9 7950X", 2900, 36000),
        new("Intel Core i9-14900K", 3100, 22000),
    ];

    // Approximate relative index, ordered/scaled to mirror UserBenchmark "Effective Speed"
    // (gaming-weighted, so e.g. the GTX 1650 edges the RX 470 despite lower raw FP32).
    // Not certified figures.
    public static readonly ReferenceGpu[] ReferenceGpus =
    [
        new("Intel UHD 620", 350),
        new("AMD RX 470 / 570", 2700),
        new("NVIDIA GTX 1650", 2950),
        new("NVIDIA RTX 3060", 6600),
        new("AMD RX 6700 XT", 8000),
        new("NVIDIA RTX 4070", 10500),
        new("NVIDIA RTX 4090", 15500),
    ];

    public static readonly string[] CpuSuggestions =
    [
        "Intel Core i3-12100", "Intel Core i5-12400F", "Intel Core i5-13600K", "Intel Core i7-12700K",
        "Intel Core i7-13700K", "Intel Core i9-13900K", "Intel Core i9-14900K",
        "AMD Ryzen 3 4100", "AMD Ryzen 5 5600X", "AMD Ryzen 5 7600X", "AMD Ryzen 7 5800X3D",
        "AMD Ryzen 7 7800X3D", "AMD Ryzen 9 7950X", "Apple M1", "Apple M2", "Apple M3 Pro",
    ];

    public static readonly string[] GpuSuggestions =
    [
        "Intel UHD Graphics 620", "Intel Iris Xe", "NVIDIA GTX 1650", "NVIDIA GTX 1660 Super",
        "NVIDIA RTX 3060", "NVIDIA RTX 3070", "NVIDIA RTX 4060", "NVIDIA RTX 4070", "NVIDIA RTX 4080",
        "NVIDIA RTX 4090", "AMD RX 6600", "AMD RX 6700 XT", "AMD RX 7800 XT", "AMD RX 7900 XTX",
    ];
}
