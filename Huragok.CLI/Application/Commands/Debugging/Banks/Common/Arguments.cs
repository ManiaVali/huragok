#if DEBUG
using System.CommandLine;

namespace Huragok.Application.Commands.Debug;

internal static class Arguments {
    internal static readonly Argument<string> BankPath =
        new(name: "bank") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "fsb file",
            Description = "Sound bank to enumerate."
        };
}
#endif