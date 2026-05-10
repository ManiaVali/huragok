
using FFMpegCore;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Pipes;
using Huragok.Data.Tags;

namespace Huragok.Utilities.Sound {
    internal class VorbisConverter {
        internal static async Task<byte[]> ConvertOGGTo(byte[] oggData, SoundOutExtension fileType) {
            if (fileType == SoundOutExtension.OGG) return oggData;

            await ValidateFFMpegInstalled();

            using var inStream = new MemoryStream(oggData);
            using var outStream = new MemoryStream();

            var input = new StreamPipeSource(inStream);
            var output = new StreamPipeSink(outStream);

            switch (fileType) {
                case SoundOutExtension.WAV:
                    await FFMpegArguments
                            .FromPipeInput(input)
                            .OutputToPipe(output, options => options
                                .WithAudioCodec("pcm_s16le")
                                .ForceFormat("wav"))
                            .ProcessAsynchronously();
                    break;

                case SoundOutExtension.MP3:
                    await FFMpegArguments
                            .FromPipeInput(input)
                            .OutputToPipe(output, options => options
                                .WithAudioCodec("libmp3lame")
                                .WithAudioBitrate(192)
                                .ForceFormat("mp3"))
                            .ProcessAsynchronously();
                    break;
            }

            return outStream.ToArray();
        }

        private static async Task<int> ValidateFFMpegInstalled() {
            string ffmpegPath = GlobalFFOptions.Current.BinaryFolder;
            Directory.CreateDirectory(ffmpegPath);

            if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe"))) {
                Logger.Message("Downloading ffmpeg portable.. please wait.", LoggerNewlineFormat.ReplaceLast);
                await FFMpegDownloader.DownloadBinaries(
                    FFMpegCore.Extensions.Downloader.Enums.FFMpegVersions.LatestAvailable,
                    FFMpegCore.Extensions.Downloader.Enums.FFMpegBinaries.FFMpeg,
                    GlobalFFOptions.Current,
                    FFMpegCore.Extensions.Downloader.Enums.SupportedPlatforms.Windows64
                );
                return 0;
            }
            return 0;
        }
    }
}