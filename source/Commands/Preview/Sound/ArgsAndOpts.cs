
using System.CommandLine;

namespace Huragok.Commands.Preview {
    public class ArgsAndOpts {
        public static readonly Option<int> SoundPitchRangeOption =
            new(["--pitch-range"], "Index of the pitch range to use when playing a sound tag. Defaults to the first pitch range.");

        public static readonly Option<int> SoundPermutationOption =
            new(["--permutation"], "Index of the permutation to use when playing a sound tag. Defaults to the first permutation.");

        public static readonly Option<bool> SoundLoopOption =
            new(["--loop"], "If passed, loop sound previews forever until exited manually.");

        public static readonly Option<int> TrackOption =
            new(["--track"], "Track to be played; defaults to the first track.");

        public static readonly Option<bool> AltTrackOption =
            new(["--alt-tracks"], "Play alternate clips.");
    }
}