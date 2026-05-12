
using Fmod5Sharp.FmodTypes;
using Huragok.Application.Logging;
using Huragok.Data.Processing.Audio;

namespace Huragok.Data.RuntimeFormats;

internal sealed class SoundPitchRange {
    internal int index;
    internal string name = string.Empty;
    internal List<SoundPermutation> permutations = new();
    internal SoundTag belongsToTag;

    internal SoundPitchRange(TagFieldBlockElement pitchRangeElement, SoundTag soundTag) {
        this.index = pitchRangeElement.ElementIndex;
        this.name = pitchRangeElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

        this.belongsToTag = soundTag;

        var permutationsBlock = pitchRangeElement.SelectFieldType<TagFieldBlock>("Block:permutations");
        foreach (var permutationElement in permutationsBlock.Cast<TagFieldBlockElement>()) {
            this.permutations.Add(new SoundPermutation(permutationElement, this));
        }
    }
}

internal sealed class SoundPermutation {
    internal int index;
    internal string name = string.Empty;
    internal float lengthSeconds;
    internal SoundPitchRange belongsToRange;

    private readonly (FmodSample sample, string samplePath) rawSampleData;

    internal byte[] SampleAsVorbisBytes {
        get {
            this.rawSampleData.sample.RebuildAsStandardFileFormat(out byte[]? bytes, out _);
            return bytes ?? throw new Exception($"Failed to rebuild sample for {this.name}.");
        }
    }

    internal string OriginalSamplePath => this.rawSampleData.samplePath;

    internal SoundPermutation(TagFieldBlockElement permutationElement, SoundPitchRange range) {
        this.index = permutationElement.ElementIndex;
        this.name = permutationElement.SelectFieldType<TagFieldElementStringID>("StringId:name").GetStringData();

        this.belongsToRange = range;

        var sampleInfo = permutationElement.SelectFieldType<TagFieldCustomSoundPlayback>("Custom:__custom_snpl_name_0"); // What is this name
        sampleInfo.RefreshFields(); // Why the hell are the fields blank without calling this?

        this.lengthSeconds = sampleInfo.SampleDuration;

        try {
            this.rawSampleData = FSBExplorer.FindInBanks(this);
        } catch (Exception e) {
            Logger.Error($"Error in {nameof(SoundPermutation)} constructor for {this.belongsToRange.belongsToTag.sourceTag.Path.RelativePath}::{this.name}: {e.Message}");
        }
    }
}

internal sealed class SoundTrack : IDisposable {
    internal int index;

    internal SoundTag? soundIn;
    internal SoundTag? soundLoop;
    internal SoundTag? soundOut;
    internal SoundTag? soundAltTransIn;
    internal SoundTag? soundAltLoop;
    internal SoundTag? soundAltTransOut;
    internal SoundTag? soundAltOut;

    internal SoundTrack(TagFieldBlockElement trackBlockElement) {
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