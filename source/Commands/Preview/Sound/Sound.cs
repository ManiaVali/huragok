using System.CommandLine;
using System.Reflection;
using Huragok.Commands.Base;
using Huragok.Data.Tags;
using Huragok.ManagedBlam;
using Huragok.Utilities.Sound;
using NAudio.Wave;
using CommonConstants = Huragok.Commands.Base.Constants;

namespace Huragok.Commands.Preview {
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

            var pitchRangeOpt = ArgsAndOpts.SoundPitchRangeOption;
            var permOpt = ArgsAndOpts.SoundPermutationOption;
            var loopOpt = ArgsAndOpts.SoundLoopOption;
            cmd.AddOption(pitchRangeOpt);
            cmd.AddOption(permOpt);
            cmd.AddOption(loopOpt);

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);
                string tag = tagInputContext.Paths.ToArray()[0];

                int pitchRange = ctx.ParseResult.GetValueForOption(pitchRangeOpt);
                int permutation = ctx.ParseResult.GetValueForOption(permOpt);
                bool loop = ctx.ParseResult.GetValueForOption(loopOpt);

                PreviewSoundFile(tag ?? "", pitchRange, permutation, loop);
            });

            return cmd;
        }

        private static void PreviewSoundFile(string soundTagFilepath, int rangeIndex, int permutationIndex, bool loop) {
            if (string.IsNullOrWhiteSpace(soundTagFilepath))
                throw new Exception(CommonConstants.NO_VALID_TAGS);

            BlamFunctions.InitializeBlam();

            try {
                if (string.IsNullOrWhiteSpace(soundTagFilepath)) 
                    throw new ArgumentNullException(nameof(soundTagFilepath));

                var tagPath = TagPath.FromPathAndExtension(BlamFunctions.GetValidTagPath(soundTagFilepath), "sound");
                if (!BlamFunctions.ValidateTag(tagPath, "sound")) 
                    throw new ArgumentException($"Sound extraction failed; tag file `{soundTagFilepath}` is invalid.");

                string tagRelPath = BlamFunctions.GetValidTagPath(soundTagFilepath);

                var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound");
                using var soundTag = new SoundTag(soundTagPath);

                if (rangeIndex > soundTag.PitchRanges.Count - 1)
                    throw new IndexOutOfRangeException($"Pitch range index too large! Sound tag only has {soundTag.PitchRanges.Count} range(s)!");
                var range = soundTag.PitchRanges[rangeIndex];

                if (permutationIndex > range.permutations.Count - 1)
                    throw new IndexOutOfRangeException($"Permutation index too large! Pitch range {range.index} only has {range.permutations.Count} range(s)!");
                var permutation = range.permutations[permutationIndex];

                var player = new VorbisSoundPlayer();

                player.Load(permutation.rawSampleData.sample.SampleBytes, loop);
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
                BlamFunctions.Teardown();
                Console.CursorVisible = true;
            }
        }
    }
}