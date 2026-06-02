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
    /// <summary>Normalizes ops/sec into a 0..N score (~1000 pts per 5M ops/sec baseline).</summary>
    public sealed class CpuScoreCalculator
    {
        private const double BaselineOpsPerSec = 5_000_000.0;

        public double Score(double opsPerSec)
            => opsPerSec / BaselineOpsPerSec * 1000.0;
    }

    /// <summary>Normalizes average FPS into a score.</summary>
    public sealed class GpuScoreCalculator
    {
        public double Score(double avgFps) => avgFps * 20.0;
    }
}
