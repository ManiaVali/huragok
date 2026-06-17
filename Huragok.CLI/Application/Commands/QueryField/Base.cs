
using System.CommandLine;
using Huragok.Blam;
using Huragok.Data.Serialization;
using Huragok.Data.Tags;

namespace Huragok.Application.Commands.Query;

internal static class Base {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "query-field",
            description: "Display the contents of a specific field in a tag."
        );

        // Common Arguments
        var tagHandler = new TagInputOptions(
            allowSingle: true,
            allowMultiple: false,
            allowDirectory: false,
            allowListFile: false
        );
        cmd.AddTagInput(tagHandler);
        var fieldArg = FieldArguments.FieldArgument;
        cmd.Add(fieldArg);

        // Command Handler
        cmd.SetAction(ctx => {
            var tagInputContext = ctx.Resolve(tagHandler);
            string fieldArgumentString = ctx.GetRequiredValue(fieldArg);

            QueryTagField(
                tagInputContext.Paths.ToArray()[0],
                fieldArgumentString
            );

            return 0;
        });

        return cmd;
    }

    private static void QueryTagField(string tagFilePath, string fieldPath) {
        if (string.IsNullOrEmpty(tagFilePath)) throw new ArgumentException($"{nameof(tagFilePath)} must not be empty!");
        tagFilePath = Path.GetFullPath(tagFilePath);

        BlamEngine.Initialize();

        var tagPath = TagPath.FromFilename(tagFilePath);
        var tagFile = new TagFile(tagPath);
        var tagField = tagFile.SelectField(fieldPath) ??
            throw new NullReferenceException("Provided tag field does not exist, or failed to be read.");

        DataSerializer.Serialize(Console.OpenStandardOutput(), TagProjector.ReadField(tagField)!, MainProgram.defaultSerializationFormat);

        BlamEngine.Teardown();
    }
}