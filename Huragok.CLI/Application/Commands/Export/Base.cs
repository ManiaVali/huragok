
using System.CommandLine;

namespace Huragok.Application.Commands.Export;

internal static class Base {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "export",
            description: "Parse and convert tags into more useful formats."
        );

        cmd.Add(Bitmap.Register());
        cmd.Add(RenderModel.Register());
        cmd.Add(Sound.Register());

        return cmd;
    }
}