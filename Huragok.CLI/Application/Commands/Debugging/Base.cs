#if DEBUG
using System.CommandLine;

namespace Huragok.Application.Commands.Debug;

internal static class Base {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "debug",
            description: $"Debug commands for {Application.Constants.PROGRAM_NAME}. Should not be visible in release builds."
        ) {
            // Command attachment
            DumpFMODInfo.Register(),
            EnumerateBank.Register(),
            ProbeDisagree.Register()
        };

        return cmd;
    }
}
#endif