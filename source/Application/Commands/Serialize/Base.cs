
using System.CommandLine;
using Huragok.Blam;
using Huragok.Utilities;
using Huragok.Utilities.Serialization;

namespace Huragok.Application.Commands.Serialize;

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

                SerializeTagData(
                    tagInputContext.Paths.ToArray()[0]
                );
            });

            return cmd;
        }

        private static void SerializeTagData(string tagFilePath) {
            if (string.IsNullOrEmpty(tagFilePath)) throw new ArgumentException($"{nameof(tagFilePath)} must not be empty!");
            tagFilePath = Path.GetFullPath(tagFilePath);

            BlamEngine.Initialize();

            var tagPath = TagPath.FromFilename(tagFilePath);
            var tagFile = new TagFile(tagPath);

            Serializer.Serialize(Console.OpenStandardOutput(), TagDataReader.ReadTag(tagFile), MainProgram.defaultSerializationFormat);

            BlamEngine.Teardown();
        }
    }