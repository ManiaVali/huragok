
using Fmod5Sharp.FmodTypes;

namespace Huragok.Commands.Sounds {
    internal static class FmodExtensions {
        internal static double LengthSeconds(this FmodSample sample) => (double)sample.Metadata.SampleCount / sample.Metadata.Frequency;
    }
}