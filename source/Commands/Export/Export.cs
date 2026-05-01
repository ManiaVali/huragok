
using System.CommandLine;

namespace Huragok.Commands.Export {
    internal static class Base {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "export",
                description: "Parse and convert tags into more useful formats."
            );

            cmd.AddCommand(Bitmap.Register());
            cmd.AddCommand(RenderModel.Register());
            cmd.AddCommand(Sound.Register());

            return cmd;
        }
    }
}