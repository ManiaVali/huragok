using System.CommandLine;
using Huragok.Commands.Base;
using Huragok.Data.Tags;
using Huragok.ManagedBlam;
using Huragok.Utilities.Sound;
using NAudio.Wave;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

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

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);
                string tag = tagInputContext.Paths.ToArray()[0];

                PreviewSoundFile(tag ?? "");
            });

            return cmd;
        }

        private static void PreviewSoundFile(string soundTagFilepath) {
            if (string.IsNullOrWhiteSpace(soundTagFilepath))
                Panic(CommonArgsAndOpts.NO_VALID_TAGS, CommonArgsAndOpts.NO_TAGS_CODE);

            BlamFunctions.InitializeBlam();

            try {
                if (string.IsNullOrWhiteSpace(soundTagFilepath)) Panic($"Sound preview failed; tag path is null!");

                var tagPath = TagPath.FromPathAndExtension(BlamFunctions.GetValidTagPath(soundTagFilepath), "sound");
                if (!BlamFunctions.ValidateTag(tagPath, "sound")) Panic($"Sound extraction failed; tag file `{soundTagFilepath}` is invalid.");

                string tagRelPath = BlamFunctions.GetValidTagPath(soundTagFilepath);

                var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound");
                using var soundTag = new SoundTag(soundTagPath);

                // hardcoded for testing
                if (soundTag.PitchRanges.Count > 1)
                    Panic("Sound tag has more than one pitch range; specify which one to play with `--pitch-range NUMBER`");


                var player = new SoundPlayer();
                player.Load(soundTag.PitchRanges[0].permutations[0].rawSampleData.bytes,
                            soundTag.PitchRanges[0].permutations[0].lengthSeconds);

                player.Play();

                Console.Write("\r sound preview: [space] pause, [left arrow] reset, [esc] exit");

                while (true) {
                    if (Console.KeyAvailable) {
                        var key = Console.ReadKey(true);

                        switch (key.Key) {
                            case ConsoleKey.Spacebar:
                                if (player.State == PlaybackState.Playing) {
                                    player.Pause();
                                    Console.Write("\r sound preview: [space] resume, [left arrow] reset, [esc] exit");
                                } else {
                                    player.Play();
                                    Console.Write("\r sound preview: [space] pause, [left arrow] reset, [esc] exit    ");
                                }
                                break;

                            case ConsoleKey.LeftArrow:
                                player.Reset();
                                break;

                            case ConsoleKey.Escape:
                                player.Dispose();
                                Console.WriteLine("\r  sound preview: exited.                                          ");
                                return;
                        }
                    }

                    if (player.State == PlaybackState.Stopped) {
                        player.Dispose();
                        Console.WriteLine("\r  sound preview: reached end of audio sample.                                          ");
                        return;
                    }

                    Thread.Sleep(50);
                }

            } finally {
                BlamFunctions.Teardown();
            }
        }
    }
}