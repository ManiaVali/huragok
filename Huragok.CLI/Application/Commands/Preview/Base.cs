
using System.CommandLine;

namespace Huragok.Application.Commands.Preview;

internal static class Base {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "preview",
            description: "Preview tag data; allows specific tag types to previewed right in the terminal."
        );

        // Common Arguments
        var tagHandler = new TagInputOptions(
            allowSingle: true,
            allowMultiple: false,
            allowDirectory: false,
            allowListFile: false
        );
        cmd.AddTagInput(tagHandler);

        // Command attachment
        cmd.Add(Sound.Register());
        cmd.Add(SoundLooping.Register());

        return cmd;
    }
}