
using System.CommandLine;

namespace Huragok.Application.Commands.Preview;

internal class SoundArguments {
    internal static readonly Option<int> SoundPitchRangeOption =
        new(name: "--pitch-range") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "index",
            DefaultValueFactory = _ => 0,
            Description = "Index of the pitch range to use when playing a sound tag. Defaults to the first pitch range."
        };

    internal static readonly Option<int> SoundPermutationOption =
        new(name: "--permutation") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "index",
            DefaultValueFactory = _ => 0,
            Description = "Index of the permutation to use when playing a sound tag. Defaults to the first permutation."
        };

    internal static readonly Option<bool> SoundLoopOption =
        new(name: "--loop") {
            Arity = ArgumentArity.ExactlyOne,
            Description = "If passed, loop sound previews forever until exited manually."
        };

    internal static readonly Option<int> TrackOption =
        new(name: "--track") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "index",
            DefaultValueFactory = _ => 0,
            Description = "Track to be played. Defaults to the first track."
        };

    internal static readonly Option<bool> AltTrackOption =
        new(name: "--alt-tracks") {
            Arity = ArgumentArity.ExactlyOne,
            Description = "Play alternate clips."
        };
}