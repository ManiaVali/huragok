
using System.CommandLine;

namespace Huragok.Application.Commands.Preview;
    internal class SoundArguments {
        internal static readonly Option<int> SoundPitchRangeOption =
            new(["--pitch-range"], "Index of the pitch range to use when playing a sound tag. Defaults to the first pitch range.");

        internal static readonly Option<int> SoundPermutationOption =
            new(["--permutation"], "Index of the permutation to use when playing a sound tag. Defaults to the first permutation.");

        internal static readonly Option<bool> SoundLoopOption =
            new(["--loop"], "If passed, loop sound previews forever until exited manually.");

        internal static readonly Option<int> TrackOption =
            new(["--track"], "Track to be played; defaults to the first track.");

        internal static readonly Option<bool> AltTrackOption =
            new(["--alt-tracks"], "Play alternate clips.");
    }