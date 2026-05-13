namespace Huragok.Data.RuntimeFormats;

internal sealed class Material {
    internal readonly long index;
    internal readonly TagPath renderMethodPath;

    internal Material(TagFieldBlockElement materialElement, string renderMethodField = "render method") {
        this.index = materialElement.ElementIndex;
        this.renderMethodPath = materialElement.SelectFieldType<TagFieldReference>(renderMethodField).Path;
    }
}