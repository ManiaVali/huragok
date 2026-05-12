using System.CommandLine;
using Huragok.Application.Logging;
using Huragok.Blam;
using Huragok.Data.Processing.Audio.Vorbis;
using Huragok.Data.RuntimeFormats;
using NAudio.Wave;

namespace Huragok.Application.Commands.Preview;

internal static class Sound {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "sound",
            description: "Preview a sound tag."
        );

        // Common Arguments
        var tagHandler = new TagInputOptions(
            allowSingle: true,
            allowMultiple: false,
            allowDirectory: false,
            allowListFile: false
        );
        cmd.AddTagInput(tagHandler);

        var pitchRangeOpt = SoundArguments.SoundPitchRangeOption;
        var permOpt = SoundArguments.SoundPermutationOption;
        var loopOpt = SoundArguments.SoundLoopOption;
        cmd.Add(pitchRangeOpt);
        cmd.Add(permOpt);
        cmd.Add(loopOpt);

        // Command Handler
        cmd.SetAction(ctx => {
            var tagInputContext = ctx.Resolve(tagHandler);
            string tag = tagInputContext.Paths.ToArray()[0];

            int pitchRange = ctx.GetValue(pitchRangeOpt);
            int permutation = ctx.GetValue(permOpt);
            bool loop = ctx.GetValue(loopOpt);

            PreviewSoundFile(tag ?? "", pitchRange, permutation, loop);
        });

        return cmd;
    }

    private static void PreviewSoundFile(string soundTagFilepath, int rangeIndex, int permutationIndex, bool loop) {
        if (string.IsNullOrWhiteSpace(soundTagFilepath))
            throw new Exception(Constants.NO_VALID_TAGS);

        BlamEngine.Initialize();

        try {
            if (string.IsNullOrWhiteSpace(soundTagFilepath))
                throw new ArgumentNullException(nameof(soundTagFilepath));

            var tagPath = TagPath.FromPathAndExtension(BlamEngine.GetValidTagPath(soundTagFilepath), "sound");
            if (!BlamEngine.ValidateTag(tagPath, "sound"))
                throw new ArgumentException($"Sound extraction failed; tag file `{soundTagFilepath}` is invalid.");

            string tagRelPath = BlamEngine.GetValidTagPath(soundTagFilepath);

            var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound");
            using var soundTag = new SoundTag(soundTagPath);

            if (rangeIndex > soundTag.PitchRanges.Count - 1)
                throw new IndexOutOfRangeException($"Pitch range index too large! Sound tag only has {soundTag.PitchRanges.Count} range(s)!");
            var range = soundTag.PitchRanges[rangeIndex];

            if (permutationIndex > range.permutations.Count - 1)
                throw new IndexOutOfRangeException($"Permutation index too large! Pitch range {range.index} only has {range.permutations.Count} range(s)!");
            var permutation = range.permutations[permutationIndex];

            var player = new VorbisSoundPlayer();

            player.Load(permutation.SampleAsVorbisBytes, loop);
            player.Play();

            bool paused = false;
            while (true) {
                Console.CursorVisible = false;
                Logger.Message($"\r> sound preview: [space] {(paused ? "resume" : "pause")}, [left arrow] reset, [esc] exit -- ({player.ProgressInteger}%{(paused ? ", paused" : "")})", LoggerNewlineFormat.ReplaceLast, writeHeader: false);

                if (Console.KeyAvailable) {
                    var key = Console.ReadKey(true);

                    switch (key.Key) {
                        case ConsoleKey.Spacebar:
                            if (player.State == PlaybackState.Playing) {
                                player.Pause();
                                paused = true;
                            } else {
                                player.Play();
                                paused = false;
                            }
                            break;

                        case ConsoleKey.LeftArrow:
                            player.Reset(loop);
                            break;

                        case ConsoleKey.Escape:
                            player.Dispose();
                            Logger.Message("\r> sound preview: exited.", LoggerNewlineFormat.ReplaceLast, writeHeader: false);
                            return;
                    }
                }

                if (player.State == PlaybackState.Stopped) {
                    player.Dispose();
                    Logger.Message("\r> sound preview: reached end of audio sample.", LoggerNewlineFormat.ReplaceLast, writeHeader: false);
                    return;
                }

                Thread.Sleep(50);
            }

        } finally {
            BlamEngine.Teardown();
            Console.CursorVisible = true;
        }
    }
}