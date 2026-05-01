
namespace Huragok.Data.IntermediateFormats.Materials {
    public sealed class IF_Material {
        public readonly long index;
        public readonly TagPath renderMethodPath;

        public IF_Material(TagFieldBlockElement materialElement, string renderMethodField = "render method") {
            this.index = materialElement.ElementIndex;
            this.renderMethodPath = materialElement.SelectFieldType<TagFieldReference>(renderMethodField).Path;
        }
    }
}