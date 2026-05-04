
using System.CommandLine;
using Huragok.Commands.Base;

namespace Huragok.Commands.Preview {
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
            cmd.AddCommand(Sound.Register());

            return cmd;
        }
    }
}