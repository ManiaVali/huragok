#if DEBUG
using System.CommandLine;

namespace Huragok.Application.Commands.Debug;

internal static class Arguments {
    internal static readonly Argument<string> BankPath =
        new("path-to-a-fmod-bank", "Sound bank to enumerate.");
}
#endif