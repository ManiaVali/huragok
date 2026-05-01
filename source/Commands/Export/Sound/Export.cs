using System.CommandLine;
using Huragok.Commands.Base;
using Huragok.Data.Tags;
using Huragok.ManagedBlam;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok.Commands.Export {

    internal static class Sound {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "sound",
                description: "Export sounds to the disk in more common formats."
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

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);
                var tagList = tagInputContext.Paths.Where(f => Path.GetExtension(f).Equals(".sound", StringComparison.OrdinalIgnoreCase));
                string outDirectory = ctx.ParseResult.GetValueForOption(outDirArg) ?? throw new ArgumentException($"Output path cannot be null.");

                DumpSoundTagData(tagList, outDirectory);
            });

            return cmd;
        }

        private static void DumpSoundTagData(IEnumerable<string> tagFilePaths, string outDirectory) {
            if (!tagFilePaths.Any()) {
                Console.Error.WriteLine("Error: No files provided -- there is nothing to do.\n   If using `--directory` or `--folder`, pass `--recurse` to look in subdirectories.");
                return;
            }

            BlamFunctions.InitializeBlam();

            foreach (string path in tagFilePaths) {
                if (string.IsNullOrWhiteSpace(path)) throw new InvalidDataException($"Sound extraction failed; one of the tag paths is null!");

                var tagPath = TagPath.FromPathAndExtension(BlamFunctions.GetValidTagPath(path), "sound");
                if (!BlamFunctions.ValidateTag(tagPath, "sound")) throw new InvalidDataException($"Sound extraction failed; tag file `{path}` is invalid.");
            }

            foreach (string path in tagFilePaths) {
                string tagRelPath = BlamFunctions.GetValidTagPath(path);

                var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound");
                using var soundTag = new SoundTag(soundTagPath);

                soundTag.TryExportToDisk(outDirectory, SoundOutExtension.OGG, out var finalOutPaths);
                foreach (string finalOutPath in finalOutPaths) {
                    Console.WriteLine(Path.GetFullPath(finalOutPath));
                }
            }

            BlamFunctions.Teardown();
        }
    }
}