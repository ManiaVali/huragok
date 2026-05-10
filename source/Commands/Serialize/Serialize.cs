
using System.CommandLine;
using Huragok.Commands.Base;
using Huragok.ManagedBlam;
using Huragok.Utilities.Serializer;

namespace Huragok.Commands.Serialize {
    internal static class Base {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "serialize",
                description: "Serialize and dump the tag data of any tag to JSON or YAML."
            );

            // Common Arguments
            var tagHandler = new TagInputOptions(
                allowSingle: true,
                allowMultiple: false,
                allowDirectory: false,
                allowListFile: false
            );
            cmd.AddTagInput(tagHandler);

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);

                DumpImportData(
                    tagInputContext.Paths.ToArray()[0]
                );
            });

            return cmd;
        }

        private static void DumpImportData(string tagFilePath) {
            if (string.IsNullOrEmpty(tagFilePath)) throw new ArgumentException($"{nameof(tagFilePath)} must not be empty!");
            tagFilePath = Path.GetFullPath(tagFilePath);

            BlamFunctions.InitializeBlam();

            var tagPath = TagPath.FromFilename(tagFilePath);
            var tagFile = new TagFile(tagPath);

            DataSerializer.Serialize(Console.OpenStandardOutput(), TagSerializer.ReadTag(tagFile), MainProgram.defaultSerializationFormat);

            BlamFunctions.Teardown();
        }
    }
}