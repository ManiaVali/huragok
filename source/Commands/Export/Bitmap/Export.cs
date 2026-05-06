
using System.CommandLine;
using Huragok.Commands.Base;
using Huragok.Commands.Bitmaps;
using Huragok.Data.Tags;
using Huragok.ManagedBlam;
using Huragok.Utilities.Imaging;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok.Commands.Export {
    internal static class Bitmap {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "bitmap",
                description: "Export bitmaps to the disk in more common formats."
            );

            // Common Arguments
            var tagHandler = new TagInputOptions(
                allowSingle: false,
                allowMultiple: true,
                allowDirectory: true,
                allowListFile: true
            );
            cmd.AddTagInput(tagHandler);
            var outDirArg = CommonArgsAndOpts.OutDir;
            cmd.AddOption(outDirArg);

            // Specific Arguments
            var bitmapHandler = new BitmapExportOptions();
            cmd.AddBitmapExport(bitmapHandler);

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);
                var tagList = tagInputContext.Paths.Where(f => Path.GetExtension(f).Equals(".bitmap", StringComparison.OrdinalIgnoreCase));

                var bitmapExportContext = ctx.ParseResult.Resolve(bitmapHandler);
                string outDirectory = ctx.ParseResult.GetValueForOption(outDirArg) ?? throw new ArgumentException($"Output path cannot be null.");

                var bitmapFlags = new BitmapExportFlags();
                bool convertCubemaps = bitmapExportContext.CubeFormat == CubemapFormat.Equirectangular;

                bitmapFlags = BitmapTag.SetFlag(bitmapFlags, BitmapExportFlags.CubemapsToSphere, convertCubemaps);
                bitmapFlags = BitmapTag.SetFlag(bitmapFlags, BitmapExportFlags.FlipGreen, bitmapExportContext.NrmFlipGreen);
                bitmapFlags = BitmapTag.SetFlag(bitmapFlags, BitmapExportFlags.ReconstructZ, bitmapExportContext.NrmReconstructZ);

                DumpBitmapTagData(
                    tagList,
                    outDirectory,
                    bitmapExportContext.ImageFormat,
                    bitmapFlags
                );
            });

            return cmd;
        }

        private static void DumpBitmapTagData(IEnumerable<string> tagFilePaths, string outDirectory, BitmapFormat fileFormat, BitmapExportFlags flags) {
            if (!tagFilePaths.Any()) {
                Logger.Message("No files provided -- there is nothing to do.\n   If using `--directory` or `--folder`, pass `--recurse` to look in subdirectories.");
                return;
            }

            BlamFunctions.InitializeBlam();

            foreach (string path in tagFilePaths) {
                if (string.IsNullOrWhiteSpace(path)) throw new InvalidDataException($"Bitmap extraction failed; one of the tag paths is null!");

                var tagPath = TagPath.FromPathAndExtension(BlamFunctions.GetValidTagPath(path), "bitmap");
                if (!BlamFunctions.ValidateTag(tagPath, "bitmap")) throw new InvalidDataException($"Bitmap extraction failed; tag file `{path}` is invalid.");
            }

            foreach (string path in tagFilePaths) {
                string tagRelPath = BlamFunctions.GetValidTagPath(path);

                var bitmTagPath = TagPath.FromPathAndExtension(tagRelPath, "bitmap");
                using var bitmTag = new BitmapTag(bitmTagPath, flags);

                bitmTag.TryExportToDisk(outDirectory, fileFormat, out var finalOutPaths);
                foreach (string finalOutPath in finalOutPaths) {
                    Logger.Message($"Saved file to `{Path.GetFullPath(finalOutPath)}`");
                }
            }

            BlamFunctions.Teardown();
        }
    }
}