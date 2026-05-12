
using System.CommandLine;
using Huragok.Application.Logging;
using Huragok.Blam;
using Huragok.Data.RuntimeFormats;
using Huragok.Data.Tags;

namespace Huragok.Application.Commands.Export;

internal static class RenderModel {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "render-model",
            description: "Export render models to more common formats."
        );

        // Common Arguments
        var tagHandler = new TagInputOptions(
            allowSingle: false,
            allowMultiple: true,
            allowDirectory: true,
            allowListFile: true
        );
        cmd.AddTagInput(tagHandler);
        var outDirArg = Commands.Arguments.OutDir;
        cmd.Add(outDirArg);

        // Specific Arguments
        var modelHandler = new RenderModelExportOptions();
        cmd.AddRenderModelExport(modelHandler);

        // Command Handler
        cmd.SetAction(ctx => {
            var tagInputContext = ctx.Resolve(tagHandler);
            var tagList = tagInputContext.Paths.Where(f => Path.GetExtension(f).Equals(".render_model", StringComparison.OrdinalIgnoreCase));

            var modelExportContext = ctx.Resolve(modelHandler);
            string outDirectory = ctx.GetValue(outDirArg) ?? throw new ArgumentException($"Output path cannot be null.");

            DumpRenderModelTagData(
                tagList,
                outDirectory,
                modelExportContext.ModelFormat,
                modelExportContext.CoordinateSystem
            );
        });

        return cmd;
    }

    private static void DumpRenderModelTagData(IEnumerable<string> tagFilePaths, string outDirectory, RenderModelFormat modelFormat, CoordinateUnit coordinateSystem) {
        if (!tagFilePaths.Any()) {
            Logger.Message("No files provided -- there is nothing to do.\n   If using `--directory` or `--folder`, pass `--recurse` to look in subdirectories.");
            return;
        }

        BlamEngine.Initialize();

        foreach (string path in tagFilePaths) {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidDataException($"Render model extraction failed; one of the tag paths is null!");

            var tagPath = TagPath.FromPathAndExtension(BlamEngine.GetValidTagPath(path), "render_model");
            if (!BlamEngine.ValidateTag(tagPath, "render_model")) throw new InvalidDataException($"Render model extraction failed; tag file `{path}` is invalid.");
        }

        foreach (string path in tagFilePaths) {

            string tagRelPath = BlamEngine.GetValidTagPath(path);

            var rmdlTagPath = TagPath.FromPathAndExtension(tagRelPath, "render_model");
            using var rmdlTag = new RenderModelTag(rmdlTagPath, coordinateSystem);

            rmdlTag.TryExportToDisk(outDirectory, modelFormat, out var finalOutPaths);
            foreach (string finalOutPath in finalOutPaths) {
                Logger.Message($"Saved file to `{Path.GetFullPath(finalOutPath)}`");
            }

        }

        BlamEngine.Teardown();
    }
}