using BenchRig.App.Core.Models.Audio;
using BenchRig.App.Core.Models.Monitor;

namespace BenchRig.App.Core.Engines.Audio
{
    /// <summary>Round-trip latency from an echo offset.</summary>
    public sealed class LatencyCalculator
    {
        public EchoLatencyResult Calculate(double roundTripMs, double correlationPeak)
            => new()
            {
                RoundTripMs = roundTripMs,
                Confidence = Math.Clamp(correlationPeak, 0, 1),
            };
    }
}

namespace BenchRig.App.Core.Engines.Monitor
{
    /// <summary>Hz from requestAnimationFrame deltas.</summary>
    public sealed class RefreshRateCalculator
    {
        public RefreshRateResult Calculate(double measuredHz, double expectedHz)
            => new()
            {
                MeasuredHz = measuredHz,
                ExpectedHz = expectedHz,
                DeltaHz = measuredHz - expectedHz,
            };

        public double FromFrameDeltas(IReadOnlyList<double> frameDeltasMs)
        {
            if (frameDeltasMs.Count == 0) return 0;
            double avg = frameDeltasMs.Average();
            return avg <= 0 ? 0 : 1000.0 / avg;
        }
    }
}

namespace BenchRig.App.Core.Engines.Compute
{
    /// <summary>
    /// Normalizes CPU work-units/sec into single/multi scores against a reference core
    /// (≈1000 single) and estimates a Geekbench-6-equivalent (single*2 calibration).
    /// </summary>
    public sealed class CpuScoreCalculator
    {
        // Reference single-core throughput (work-units/sec) that maps to a score of 1000.
        // Calibrated against a measured mid-range desktop core; values are relative/estimated.
        private const double ReferenceSingleUps = 7000.0;
        private const double GeekbenchFactor = 1.5;   // 1000 score ≈ GB6 single ~1500

        public double SingleScore(double singleUps) => singleUps / ReferenceSingleUps * 1000.0;
        public double MultiScore(double multiUps) => multiUps / ReferenceSingleUps * 1000.0;
        public double EstGeekbench(double score) => score * GeekbenchFactor;
    }

    /// <summary>
    /// GPU score from sustained FPS weighted by the rendered workload
    /// (pixels × shader complexity), normalized to a reference index.
    /// </summary>
    public sealed class GpuScoreCalculator
    {
        // Score ∝ real GPU work/sec (width*height*raymarchSteps*fps) for the volumetric
        // "Volume Shader" benchmark, with fps measured via a genuine GPU sync (readPixels).
        // Calibrated against a measured AMD RX 470 (≈16.9 fps at 1080p/96 steps ≈ 3.37e9
        // work/sec → ≈2700) so it lands just under the GTX 1650 in the UserBenchmark-ordered
        // ReferenceGpus table. Approximate index, not certified.
        private const double Divisor = 1.25e6;

        public double Score(double avgFps, int width, int height, int complexity)
        {
            double workPerSec = (double)width * height * Math.Max(1, complexity) * avgFps;
            // Per-resolution normalization so every preset reports the SAME tier for a given
            // GPU. Without it, raw throughput climbs with resolution (low-res presets lose time
            // to per-frame sync/setup overhead; high-res presets saturate the GPU better), so a
            // card would creep up a tier at 4K. Factors calibrated on a measured RX 470 across
            // all four presets (720p→4K: 2098/2693/2872/2996 → all ≈2700).
            double mpx = width * (double)height / 1_000_000.0;
            double calib = mpx < 1.5 ? 1.287    // 720p
                         : mpx < 3.0 ? 1.003    // 1080p
                         : mpx < 6.0 ? 0.940    // 1440p
                         :             0.901;   // 4K
            return workPerSec / Divisor * calib;
        }
    }

    /// <summary>Memory score from bandwidth (GB/s) and peak stable allocation.</summary>
    public sealed class MemoryScoreCalculator
    {
        public double Score(double writeGBs, double readGBs, double peakMb)
        {
            double bandwidth = (writeGBs + readGBs) / 2.0;     // GB/s
            return bandwidth * 1000.0 + peakMb;                // simple composite
        }
    }
}
