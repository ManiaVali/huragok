
using System.CommandLine;

namespace Huragok.Commands.Export {
    internal static class ArgsAndOpts {
        internal static readonly Option<string> AudioFormatOption =
            new(["-f", "--audio-format"], "Format which the bitmap should be written to. One of `ogg`, `wav`, `mp3` and `aif`.");
    }
}