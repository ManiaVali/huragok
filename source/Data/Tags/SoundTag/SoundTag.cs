
using Huragok.Data.IntermediateFormats.Sound;

namespace Huragok.Data.Tags {
    public enum SoundOutExtension {
        OGG
    }

    public sealed class SoundTag : BaseTag<SoundOutExtension> {
        #region Properties/Fields
        protected override string TagExtension => "sound";
        private TagFieldBlock BlockPitchRanges => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:pitch ranges");
        private readonly List<IF_PitchRange> pitchRanges = new();
        public IReadOnlyList<IF_PitchRange> PitchRanges => this.pitchRanges;
        #endregion

        #region Sound Decoding
        public SoundTag(TagPath tagPath) : base(tagPath) {
            foreach (var range in this.BlockPitchRanges.Elements.Cast<TagFieldBlockElement>()) {
                this.pitchRanges.Add(new IF_PitchRange(range, this));
            }
        }
        #endregion

        #region Export Funcs
        public override bool TryExportToDisk(string outputDirectory, SoundOutExtension fileExtension, out List<string> finalFileLocations) {
            string extension = fileExtension.ToString().ToLower() ?? "ogg";
            List<string> outPaths = new();

            foreach (var range in this.PitchRanges) {
                foreach (var perm in range.permutations) {
                    if (perm.rawSampleData.bytes.Length == 0) continue;
                    string outPath = Path.ChangeExtension(Path.Combine(outputDirectory, perm.rawSampleData.samplePath), extension);
                    // If someone's putting this on the root of their drive GetDirectoryName comes back null
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? outputDirectory);
                    
                    File.WriteAllBytes(outPath, perm.rawSampleData.bytes);
                    outPaths.Add(outPath);
                }
            }

            finalFileLocations = outPaths;
            return true;
        }

        public override string BuildOutputPath(string outputDirectory, string extension) => throw new NotSupportedException($"{nameof(BuildOutputPath)} not supported; use BuildSoundOutputPath.");
        #endregion
    }
}