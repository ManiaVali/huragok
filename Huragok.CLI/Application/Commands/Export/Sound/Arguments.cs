
using System.CommandLine;
using Huragok.Data.RuntimeFormats;

namespace Huragok.Application.Commands.Export;

internal static class SoundArguments {
    internal static readonly Option<string> AudioFormatOption =
        new(name: "--audio-format", aliases: ["-f"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "ogg, wav, mp3 or aif",
            DefaultValueFactory = _ => SoundOutExtension.OGG.ToString().ToLower(),
            Description = "Format which the bitmap should be written to."
        };
}
