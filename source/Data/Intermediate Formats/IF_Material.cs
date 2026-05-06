
namespace Huragok.Data.IntermediateFormats.Materials {
    internal sealed class IF_Material {
        internal readonly long index;
        internal readonly TagPath renderMethodPath;

        internal IF_Material(TagFieldBlockElement materialElement, string renderMethodField = "render method") {
            this.index = materialElement.ElementIndex;
            this.renderMethodPath = materialElement.SelectFieldType<TagFieldReference>(renderMethodField).Path;
        }
    }
}