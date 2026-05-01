
using Fmod5Sharp.FmodTypes;
using Huragok.Data.Tags;
using Huragok.Utilities.Sound;

namespace Huragok.Data.IntermediateFormats.Sound {
    public sealed class IF_PitchRange {
        public int index;
        public string name = string.Empty;
        public List<IF_SoundPermutation> permutations = new();
        public SoundTag belongsToTag;

        public IF_PitchRange(TagFieldBlockElement pitchRangeElement, SoundTag soundTag) {
            this.index = pitchRangeElement.ElementIndex;
            this.name = pitchRangeElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

            this.belongsToTag = soundTag;

            var permutationsBlock = pitchRangeElement.SelectFieldType<TagFieldBlock>("Block:permutations");
            foreach (var permutationElement in permutationsBlock.Cast<TagFieldBlockElement>()) {
                this.permutations.Add(new IF_SoundPermutation(permutationElement, this));
            }
        }
    }

    public sealed class IF_SoundPermutation {
        public int index;
        public string name = string.Empty;
        public float lengthSeconds;
        public IF_PitchRange belongsToRange;

        internal readonly (byte[] bytes, string samplePath) rawSampleData;

        public IF_SoundPermutation(TagFieldBlockElement permutationElement, IF_PitchRange range) {
            this.index = permutationElement.ElementIndex;
            this.name = permutationElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

            this.belongsToRange = range;

            var sampleInfo = permutationElement.SelectFieldType<TagFieldCustomSoundPlayback>("Custom:__custom_snpl_name_0"); // What is this name
            sampleInfo.RefreshFields(); // Why the hell are the fields blank without calling this?

            this.lengthSeconds = sampleInfo.SampleDuration;

            (byte[] data, string name, string originalSamplePath) sampleData = (Array.Empty<byte>(), string.Empty, string.Empty);
            try {
                sampleData = FSBExplorer.FindSample(this);
            } catch (Exception e) {
                Console.Error.WriteLine($"Error: {e.Message}");
            }

            this.rawSampleData = (sampleData.data, sampleData.originalSamplePath);
        }
    }
}