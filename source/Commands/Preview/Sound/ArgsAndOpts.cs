
using System.CommandLine;

namespace Huragok.Commands.Preview {
    public class ArgsAndOpts {
        public static readonly Option<int> SoundPitchRangeOption =
            new(["--pitch-range"], "Index of the pitch range to use when playing a sound tag. Required if there is more than one range.");

        public static readonly Option<int> SoundPermutationOption =
            new(["--permutation"], "Index of the permutation to use when playing a sound tag. Required if there is more than one permutation.");

        public static readonly Option<bool> SoundLoopOption =
            new(["--loop"], "If passed, loop sound previews forever until exited manually.");
    }
}