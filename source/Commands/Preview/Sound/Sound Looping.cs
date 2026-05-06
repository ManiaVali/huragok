using System.CommandLine;
using Huragok.Commands.Base;
using Huragok.Data.Tags;
using Huragok.ManagedBlam;
using Huragok.Utilities.Sound;
using NAudio.Wave;
using CommonConstants = Huragok.Commands.Base.Constants;

namespace Huragok.Commands.Preview {
    internal enum PlaybackPhase {
        In,
        Loop,
        Out,
        Done
    }

    internal static class SoundLooping {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "sound-looping",
                description: "Preview a sound_looping tag."
            );

            // Common Arguments
            var tagHandler = new TagInputOptions(
                allowSingle: true,
                allowMultiple: false,
                allowDirectory: false,
                allowListFile: false
            );
            cmd.AddTagInput(tagHandler);

            var trackOption = ArgsAndOpts.TrackOption;
            cmd.AddOption(trackOption);
            var altTracksOption = ArgsAndOpts.AltTrackOption;
            cmd.AddOption(altTracksOption);

            // Command Handler
            cmd.SetHandler(ctx => {
                var tagInputContext = ctx.ParseResult.Resolve(tagHandler);
                string tag = tagInputContext.Paths.ToArray()[0];

                int trackOpt = ctx.ParseResult.GetValueForOption(trackOption);
                bool playingAltTracks = ctx.ParseResult.GetValueForOption(altTracksOption);

                PreviewLoopingSoundFile(tag ?? "", trackOpt, playingAltTracks);
            });

            return cmd;
        }

        private static void PreviewLoopingSoundFile(string soundTagFilepath, int trackIndex, bool altTracks) {
            if (string.IsNullOrWhiteSpace(soundTagFilepath))
                throw new Exception(CommonConstants.NO_VALID_TAGS);

            BlamFunctions.InitializeBlam();

            try {
                if (string.IsNullOrWhiteSpace(soundTagFilepath)) 
                    throw new ArgumentNullException(nameof(soundTagFilepath));

                var tagPath = TagPath.FromPathAndExtension(BlamFunctions.GetValidTagPath(soundTagFilepath), "sound_looping");
                if (!BlamFunctions.ValidateTag(tagPath, "sound_looping")) 
                    throw new ArgumentException($"Sound extraction failed; tag file `{soundTagFilepath}` is invalid.");

                string tagRelPath = BlamFunctions.GetValidTagPath(soundTagFilepath);

                var soundTagPath = TagPath.FromPathAndExtension(tagRelPath, "sound_looping");
                using var soundLoopingTag = new SoundLoopingTag(soundTagPath);

                if (trackIndex > soundLoopingTag.Tracks.Count - 1)
                    throw new IndexOutOfRangeException($"Track index too large! Sound tag only has {soundLoopingTag.Tracks.Count} range(s)!");

                var track = soundLoopingTag.Tracks[trackIndex];
                var player = new VorbisSoundPlayer();

                SoundTag? inClip = null;
                SoundTag? loopClip = null;
                SoundTag? outClip = null;

                if (!altTracks) {
                    if (track.soundIn is not null)
                        inClip = track.soundIn;
                    if (track.soundLoop is not null)
                        loopClip = track.soundLoop;
                    if (track.soundOut is not null)
                        outClip = track.soundOut;
                } else {
                    if (track.soundAltTransIn is not null)
                        inClip = track.soundAltTransIn;
                    if (track.soundAltLoop is not null)
                        loopClip = track.soundAltLoop;
                    if (track.soundAltTransOut is not null)
                        outClip = track.soundAltTransOut;
                }

                if (loopClip is null)
                    throw new InvalidDataException($"Looping sound tag `{soundLoopingTag.sourceTag.Path.ShortNameWithExtension}` has no loop track!");

                // holy booleans, batman!
                bool paused = false;
                bool exitTransitionRequested = false;
                var phase = PlaybackPhase.In;                
                bool startedIn = false;
                bool startedOut = false;
                double lastProgress = 0;
                bool exitedByUser = false;
                while (phase != PlaybackPhase.Done) {
                    Console.CursorVisible = false;
                    string stage = phase switch {
                        PlaybackPhase.In => "in",
                        PlaybackPhase.Loop => exitTransitionRequested ? "loop, waiting to exit..." : "loop",
                        PlaybackPhase.Out => "out",
                        PlaybackPhase.Done => "done",
                        _ => throw new NotImplementedException(),
                    };

                    Logger.Message($"\r> looping sound preview: [space] {(paused ? "resume" : "pause")}{(phase == PlaybackPhase.Loop && !exitTransitionRequested ? ", [right arrow] transition out" : "")}, [esc] exit -- ({stage})", LoggerNewlineFormat.ReplaceLast, writeHeader: false);

                    HandleInput();

                    switch (phase) {
                        case PlaybackPhase.In:
                            if (inClip != null) {
                                if (player.State == PlaybackState.Stopped && !startedIn) {
                                    player.Load(inClip.PitchRanges[0].permutations[0].SampleAsVorbisBytes);
                                    player.Play();
                                    startedIn = true;
                                } else if (startedIn && player.State == PlaybackState.Stopped) {
                                    phase = PlaybackPhase.Loop;
                                }
                            } else {
                                phase = PlaybackPhase.Loop;
                            }
                            break;

                        case PlaybackPhase.Loop:
                            if (player.State == PlaybackState.Stopped && !exitTransitionRequested) {
                                player.Load(loopClip!.PitchRanges[0].permutations[0].SampleAsVorbisBytes, looping: true);
                                player.Play();
                            }

                            double currentProgress = player.Progress;
                            bool looped = currentProgress < lastProgress;

                            if (exitTransitionRequested && looped) {
                                player.Dispose();
                                phase = PlaybackPhase.Out;
                                startedOut = false;
                            }

                            lastProgress = currentProgress;
                            break;

                        case PlaybackPhase.Out:
                            if (outClip != null) {
                                if (player.State == PlaybackState.Stopped && !startedOut) {
                                    player.Load(outClip.PitchRanges[0].permutations[0].SampleAsVorbisBytes);
                                    player.Play();
                                    startedOut = true;
                                } else if (startedOut && player.State == PlaybackState.Stopped) {
                                    phase = PlaybackPhase.Done;
                                }
                            } else {
                                phase = PlaybackPhase.Done;
                            }
                            break;
                    }

                    Thread.Sleep(10);
                }

                player.Dispose();
                Logger.Message(!exitedByUser ? "\r> looping sound preview: reached end of audio sample." : "\r> looping sound preview: exited.", LoggerNewlineFormat.ReplaceLast, writeHeader: false);

                void HandleInput() {
                    if (!Console.KeyAvailable) return;

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

                        case ConsoleKey.RightArrow:
                            if (phase == PlaybackPhase.Loop)
                                exitTransitionRequested = true;
                            break;

                        case ConsoleKey.Escape:
                            player.Dispose();
                            phase = PlaybackPhase.Done;
                            exitedByUser = true;
                            break;
                    }
                }
            } finally {
                BlamFunctions.Teardown();
                Console.CursorVisible = true;
            }
        }
    }
}