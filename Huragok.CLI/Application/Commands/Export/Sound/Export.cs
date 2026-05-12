using System.CommandLine;
using Huragok.Application.Logging;
using Huragok.Blam;
using Huragok.Data.RuntimeFormats;

namespace Huragok.Application.Commands.Export;

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
        var outDirOpt = Commands.Arguments.OutDir;
        cmd.Add(outDirOpt);
        var outFormatOption = SoundArguments.AudioFormatOption;
        cmd.Add(outFormatOption);

        // Command Handler
        cmd.SetAction(ctx => {
            var tagInputContext = ctx.Resolve(tagHandler);
            var tagList = tagInputContext.Paths.Where(f => Path.GetExtension(f).Equals(".sound", StringComparison.OrdinalIgnoreCase));
            string outDirectory = ctx.GetRequiredValue(outDirOpt);
            string outFmt = ctx.GetRequiredValue(outFormatOption).ToLower();

            var extension = outFmt switch {
                "ogg" => SoundOutExtension.OGG,
                "wav" => SoundOutExtension.WAV,
                "mp3" => SoundOutExtension.MP3,
                _ => throw new ArgumentException($"Unsupported file format `{outFmt}`.")
            };

            DumpSoundTagData(tagList, outDirectory, extension);
        });

        return cmd;
    }

    private static void DumpSoundTagData(IEnumerable<string> tagFilePaths, string outDirectory, SoundOutExtension extension) {
        if (!tagFilePaths.Any()) {
            Logger.Message("No files provided -- there is nothing to do.\n   If using `--directory` or `--folder`, pass `--recurse` to look in subdirectories.");
            return;
        }

        BlamEngine.Initialize();

        foreach (string path in tagFilePaths) {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidDataException($"Sound extraction failed; one of the tag paths is null!");

            var tagPath = TagPath.FromPathAndExtension(BlamEngine.GetValidTagPath(path), "sound");
            if (!BlamEngine.ValidateTag(tagPath, "sound")) throw new InvalidDataException($"Sound extraction failed; tag file `{path}` is invalid.");
        }

        foreach (string path in tagFilePaths) {
            string tagRelPath = BlamEngine.GetValidTagPath(path);

            var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound");
            using var soundTag = new SoundTag(soundTagPath);

            soundTag.TryExportToDisk(outDirectory, extension, out var finalOutPaths);
            foreach (string finalOutPath in finalOutPaths) {
                Logger.Message($"Saved file to `{Path.GetFullPath(finalOutPath)}`");
            }
        }

        BlamEngine.Teardown();
    }
}