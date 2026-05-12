
using System.CommandLine;

namespace Huragok.Application.Commands.Export;

internal static class Base {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "export",
            description: "Parse and convert tags into more useful formats."
        ) {
            // Command attachment
            Bitmap.Register(),
            RenderModel.Register(),
            Sound.Register()
        };

        return cmd;
    }
}