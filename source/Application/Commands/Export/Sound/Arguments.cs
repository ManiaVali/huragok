
using System.CommandLine;

namespace Huragok.Application.Commands.Export;

internal static class SoundArguments {
    internal static readonly Option<string> AudioFormatOption =
        new(["-f", "--audio-format"], "Format which the bitmap should be written to. One of `ogg`, `wav`, `mp3` and `aif`.");
}
