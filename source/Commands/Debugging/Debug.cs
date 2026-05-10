#if DEBUG
using System.CommandLine;
using Huragok.Utilities;

namespace Huragok.Commands.Debug {
    internal static class Base {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "debug",
                description: $"Debug commands for {GlobalConstants.PROGRAM_NAME}. Should not be visible in release builds."
            );

            // Command attachment
            cmd.AddCommand(DumpFMODInfo.Register());
            cmd.AddCommand(EnumerateBank.Register());
            cmd.AddCommand(ProbeDisagree.Register());

            return cmd;
        }
    }
}
#endif