using System.Numerics;
using Huragok.Data.IntermediateFormats.Armature;
using Huragok.Utilities;

namespace Huragok.Data.IntermediateFormats.Markers {
    internal enum IF_Marker_GroupType {
        Model,
        Effects,
        Target,
        Garbage,
        Hint
    }

    internal sealed class IF_MarkerGroup {
        internal readonly long index;
        internal readonly string name;

        internal readonly List<IF_Marker> markers = new();
        internal readonly IF_Marker_GroupType groupType;

        internal IF_MarkerGroup(TagFieldBlockElement markerGroupElement, string markerGroupNameField = "name", string markersFieldBlock = "markers") {
            this.index = markerGroupElement.ElementIndex;
            this.name = markerGroupElement.SelectFieldType<TagFieldElement>(markerGroupNameField).GetStringData();

            this.groupType = this.name switch {
                var n when n.StartsWith("fx_") => IF_Marker_GroupType.Effects,
                var n when n.StartsWith("target_") => IF_Marker_GroupType.Target,
                var n when n.StartsWith("garbage_") => IF_Marker_GroupType.Garbage,
                var n when n.StartsWith("hint_") => IF_Marker_GroupType.Hint,
                _ => IF_Marker_GroupType.Model
            };

            foreach (var e in markerGroupElement.SelectFieldType<TagFieldBlock>(markersFieldBlock).Elements.Cast<TagFieldBlockElement>()) {
                this.markers.Add(new IF_Marker(e));
            }
        }

        internal List<IF_Marker> MarkersForNode(IF_ArmatureNode armatureNode) => this.markers.Where(m => m.nodeIndex == armatureNode.index).ToList();
    }

    internal sealed class IF_Marker {
        internal readonly long index;
        internal readonly long regionIndex;
        internal readonly long permutationIndex;
        internal readonly long nodeIndex;

        internal readonly Vector3 translation;
        internal readonly Quaternion rotation;
        internal readonly float scale;
        internal readonly Vector3? direction;

        internal readonly bool nodeRelativePosition;

        internal IF_Marker(TagFieldBlockElement markerElement) {
            this.index = markerElement.ElementIndex;
            this.regionIndex = markerElement.SelectFieldType<TagFieldElementInteger>("region index").Data;
            this.permutationIndex = markerElement.SelectFieldType<TagFieldElementInteger>("permutation index").Data;
            this.nodeIndex = markerElement.SelectFieldType<TagFieldElementInteger>("node index").Data;

            this.translation = TagDataReader.FromTagFloatArray<Vector3>(markerElement.SelectFieldType<TagFieldElementArraySingle>("translation"));
            this.rotation = TagDataReader.FromTagFloatArray<Quaternion>(markerElement.SelectFieldType<TagFieldElementArraySingle>("rotation"));
            this.direction = TagDataReader.FromTagFloatArray<Vector3>(markerElement.SelectFieldType<TagFieldElementArraySingle>("direction"));
            this.scale = markerElement.SelectFieldType<TagFieldElementSingle>("scale");

            var flags = markerElement.SelectFieldType<TagFieldFlags>("flags");
            this.nodeRelativePosition = flags.TestBit("has node relative direction");
        }
    }
}