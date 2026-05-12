

using Huragok.Data.RuntimeFormats;

namespace Huragok.Data.Tags;

internal sealed class SoundLoopingTag : BaseTag<SoundOutExtension> {
    #region Properties/Fields
    protected override string TagExtension => "sound_looping";
    private TagFieldBlock BlockTracks => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:tracks");

    internal List<SoundTrack> Tracks = new();
    #endregion

    #region Sound Decoding
    internal SoundLoopingTag(TagPath tagPath) : base(tagPath) {
        foreach (var element in this.BlockTracks.Cast<TagFieldBlockElement>()) {
            this.Tracks.Add(new SoundTrack(element));
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        foreach (var track in this.Tracks) {
            track.Dispose();
        }
    }
    #endregion
}