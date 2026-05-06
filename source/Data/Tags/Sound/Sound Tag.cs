
using Huragok.Data.IntermediateFormats.Sound;
using Huragok.Utilities.Sound;

namespace Huragok.Data.Tags {
    internal enum SoundOutExtension {
        OGG,
        WAV,
        MP3
    }

    internal sealed class SoundTag : BaseTag<SoundOutExtension> {
        #region Properties/Fields
        protected override string TagExtension => "sound";
        private TagFieldBlock BlockPitchRanges => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:pitch ranges");
        private readonly List<IF_PitchRange> pitchRanges = new();
        internal IReadOnlyList<IF_PitchRange> PitchRanges => this.pitchRanges;
        #endregion

        #region Sound Decoding
        internal SoundTag(TagPath tagPath) : base(tagPath) {
            foreach (var range in this.BlockPitchRanges.Elements.Cast<TagFieldBlockElement>()) {
                this.pitchRanges.Add(new IF_PitchRange(range, this));
            }
        }
        #endregion

        #region Export Funcs
        internal override bool TryExportToDisk(string outputDirectory, SoundOutExtension fileType, out List<string> finalFileLocations) {
            string extension = fileType.ToString().ToLower() ?? "ogg";
            List<string> outPaths = new();

            foreach (var range in this.PitchRanges) {
                foreach (var perm in range.permutations) {
                    if (perm.rawSampleData.sample == null || perm.rawSampleData.sample.SampleBytes == null) continue;
                    if (perm.rawSampleData.sample.SampleBytes.Length == 0) continue;
                    byte[] data = VorbisConverter.ConvertOGGTo(perm.rawSampleData.sample.SampleBytes, fileType).Result;

                    string outPath = Path.ChangeExtension(Path.Combine(outputDirectory, perm.rawSampleData.samplePath), extension);
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
}