
using Huragok.Data.Tags;
using Huragok.Utilities.Sound;

namespace Huragok.Data.IntermediateFormats.Sound {
    internal sealed class IF_PitchRange {
        internal int index;
        internal string name = string.Empty;
        internal List<IF_SoundPermutation> permutations = new();
        internal SoundTag belongsToTag;

        internal IF_PitchRange(TagFieldBlockElement pitchRangeElement, SoundTag soundTag) {
            this.index = pitchRangeElement.ElementIndex;
            this.name = pitchRangeElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

            this.belongsToTag = soundTag;

            var permutationsBlock = pitchRangeElement.SelectFieldType<TagFieldBlock>("Block:permutations");
            foreach (var permutationElement in permutationsBlock.Cast<TagFieldBlockElement>()) {
                this.permutations.Add(new IF_SoundPermutation(permutationElement, this));
            }
        }
    }

    internal sealed class IF_SoundPermutation {
        internal int index;
        internal string name = string.Empty;
        internal float lengthSeconds;
        internal IF_PitchRange belongsToRange;

        internal readonly (Fmod5Sharp.FmodTypes.FmodSample sample, string samplePath) rawSampleData;

        internal IF_SoundPermutation(TagFieldBlockElement permutationElement, IF_PitchRange range) {
            this.index = permutationElement.ElementIndex;
            this.name = permutationElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

            this.belongsToRange = range;

            var sampleInfo = permutationElement.SelectFieldType<TagFieldCustomSoundPlayback>("Custom:__custom_snpl_name_0"); // What is this name
            sampleInfo.RefreshFields(); // Why the hell are the fields blank without calling this?

            this.lengthSeconds = sampleInfo.SampleDuration;

            try {
                this.rawSampleData = FSBExplorer.FindInBank(this);
            } catch (Exception e) {
                Logger.Error($"Error in {nameof(IF_SoundPermutation)} constructor for {this.name}: {e.Message}");
            }
        }
    }

    internal sealed class IF_Track : IDisposable {
        internal int index;

        internal SoundTag? soundIn;
        internal SoundTag? soundLoop;
        internal SoundTag? soundOut;
        internal SoundTag? soundAltTransIn;
        internal SoundTag? soundAltLoop;
        internal SoundTag? soundAltTransOut;
        internal SoundTag? soundAltOut;

        internal IF_Track(TagFieldBlockElement trackBlockElement) {
            this.index = trackBlockElement.ElementIndex;

            var soundInRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:in").Path;
            if (soundInRef is not null) this.soundIn = new SoundTag(soundInRef);

            var soundLoopRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:loop").Path;
            if (soundLoopRef is not null) this.soundLoop = new SoundTag(soundLoopRef);

            var soundOutRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:out").Path;
            if (soundOutRef is not null) this.soundOut = new SoundTag(soundOutRef);

            var soundAltTransInRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:alt trans in").Path;
            if (soundAltTransInRef is not null) this.soundAltTransIn = new SoundTag(soundAltTransInRef);

            var soundAltLoop = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:alt loop").Path;
            if (soundAltLoop is not null) this.soundAltLoop = new SoundTag(soundAltLoop);

            var soundAltTransOutRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:alt trans out").Path;
            if (soundAltTransOutRef is not null) this.soundAltTransOut = new SoundTag(soundAltTransOutRef);

            var soundAltOutRef = trackBlockElement.SelectFieldType<TagFieldReference>("Reference:alt out").Path;
            if (soundAltOutRef is not null) this.soundAltOut = new SoundTag(soundAltOutRef);
        }

        public void Dispose() {
            this.soundIn?.Dispose();
            this.soundLoop?.Dispose();
            this.soundOut?.Dispose();
            this.soundAltTransIn?.Dispose();
            this.soundAltLoop?.Dispose();
            this.soundAltTransOut?.Dispose();
            this.soundAltOut?.Dispose();
        }
    }
}