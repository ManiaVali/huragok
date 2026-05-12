using System.Numerics;
using Huragok.Data.Tags;

namespace Huragok.Data.RuntimeFormats;

internal enum MarkerGroupType {
    Model,
    Effects,
    Target,
    Garbage,
    Hint
}

internal sealed class MarkerGroup {
    internal readonly long index;
    internal readonly string name;

    internal readonly List<Marker> markers = new();
    internal readonly MarkerGroupType groupType;

    internal MarkerGroup(TagFieldBlockElement markerGroupElement, string markerGroupNameField = "name", string markersFieldBlock = "markers") {
        this.index = markerGroupElement.ElementIndex;
        this.name = markerGroupElement.SelectFieldType<TagFieldElement>(markerGroupNameField).GetStringData();

        this.groupType = this.name switch {
            var n when n.StartsWith("fx_") => MarkerGroupType.Effects,
            var n when n.StartsWith("target_") => MarkerGroupType.Target,
            var n when n.StartsWith("garbage_") => MarkerGroupType.Garbage,
            var n when n.StartsWith("hint_") => MarkerGroupType.Hint,
            _ => MarkerGroupType.Model
        };

        foreach (var e in markerGroupElement.SelectFieldType<TagFieldBlock>(markersFieldBlock).Elements.Cast<TagFieldBlockElement>()) {
            this.markers.Add(new Marker(e));
        }
    }

    internal List<Marker> MarkersForNode(ArmatureNode armatureNode) => this.markers.Where(m => m.nodeIndex == armatureNode.index).ToList();
}

internal sealed class Marker {
    internal readonly long index;
    internal readonly long regionIndex;
    internal readonly long permutationIndex;
    internal readonly long nodeIndex;

    internal readonly Vector3 translation;
    internal readonly Quaternion rotation;
    internal readonly float scale;
    internal readonly Vector3? direction;

    internal readonly bool nodeRelativePosition;

    internal Marker(TagFieldBlockElement markerElement) {
        this.index = markerElement.ElementIndex;
        this.regionIndex = markerElement.SelectFieldType<TagFieldElementInteger>("region index").Data;
        this.permutationIndex = markerElement.SelectFieldType<TagFieldElementInteger>("permutation index").Data;
        this.nodeIndex = markerElement.SelectFieldType<TagFieldElementInteger>("node index").Data;

        this.translation = TagProjector.FromTagFloatArray<Vector3>(markerElement.SelectFieldType<TagFieldElementArraySingle>("translation"));
        this.rotation = TagProjector.FromTagFloatArray<Quaternion>(markerElement.SelectFieldType<TagFieldElementArraySingle>("rotation"));
        this.direction = TagProjector.FromTagFloatArray<Vector3>(markerElement.SelectFieldType<TagFieldElementArraySingle>("direction"));
        this.scale = markerElement.SelectFieldType<TagFieldElementSingle>("scale");

        var flags = markerElement.SelectFieldType<TagFieldFlags>("flags");
        this.nodeRelativePosition = flags.TestBit("has node relative direction");
    }
}