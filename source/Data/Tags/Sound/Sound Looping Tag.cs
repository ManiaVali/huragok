
using Huragok.Data.IntermediateFormats.Sound;

namespace Huragok.Data.Tags {
    public sealed class SoundLoopingTag : BaseTag<SoundOutExtension> {
        #region Properties/Fields
        protected override string TagExtension => "sound_looping";
        private TagFieldBlock BlockTracks => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:tracks");

        public List<IF_Track> Tracks = new();
        #endregion

        #region Sound Decoding
        public SoundLoopingTag(TagPath tagPath) : base(tagPath) {
            foreach (var element in this.BlockTracks.Cast<TagFieldBlockElement>()) {
                this.Tracks.Add(new IF_Track(element));
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
}