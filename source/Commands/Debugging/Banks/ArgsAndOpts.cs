#if DEBUG
using System.CommandLine;

namespace Huragok.Commands.Debug {
    internal static class ArgsAndOpts {
        internal static readonly Argument<string> BankPath =
            new("path-to-a-fmod-bank", "Sound bank to enumerate.");
    }
}
#endif