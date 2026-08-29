
using Huragok.Application.Logging;
using Huragok.Data.Processing.Audio.Vorbis;
using Huragok.Data.Tags;

namespace Huragok.Data.RuntimeFormats;

internal enum SoundOutExtension : byte {
    OGG,
    WAV,
    MP3
}

internal sealed class SoundTag : BaseTag<SoundOutExtension> {
    #region Properties/Fields
    protected override string TagExtension => "sound";
    private TagFieldBlock BlockPitchRanges => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:pitch ranges");
    private readonly List<SoundPitchRange> pitchRanges = new();
    internal IReadOnlyList<SoundPitchRange> PitchRanges => this.pitchRanges;
    #endregion

    #region Sound Decoding
    internal SoundTag(TagPath tagPath) : base(tagPath) {
        foreach (var range in this.BlockPitchRanges.Elements.Cast<TagFieldBlockElement>()) {
            this.pitchRanges.Add(new SoundPitchRange(range, this));
        }
    }
    #endregion

    #region Export Funcs
    internal override bool TryExportToDisk(string outputDirectory, SoundOutExtension fileType, out List<string> finalFileLocations) {
        Logger.Debug($"{this.TagName}: Disk export requested.");

        string extension = fileType.ToString().ToLower() ?? "ogg";
        List<string> outPaths = new();

        foreach (var range in this.PitchRanges) {
            foreach (var perm in range.permutations) {
                if (perm.SampleAsVorbisBytes.Length == 0) continue;

                byte[] data = VorbisConverter.ConvertOGGTo(perm.SampleAsVorbisBytes, fileType).Result;

                string outPath = Path.ChangeExtension(Path.Combine(outputDirectory, perm.OriginalSamplePath), extension);
                // If someone's putting this on the root of their drive GetDirectoryName comes back null
                Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? outputDirectory);

                File.WriteAllBytes(outPath, data);
                outPaths.Add(outPath);
            }
        }

        finalFileLocations = outPaths;
        return true;
    }

    internal override string BuildOutputPath(string outputDirectory, string extension) => throw new NotSupportedException($"{nameof(BuildOutputPath)} not supported; use BuildSoundOutputPath.");
    #endregion
}