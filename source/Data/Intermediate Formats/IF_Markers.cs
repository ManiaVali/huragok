using System.Numerics;
using Huragok.Data.IntermediateFormats.Armature;
using Huragok.Data.IntermediateFormats.Coordinates;
using Huragok.Utilities;

namespace Huragok.Data.IntermediateFormats.Markers {
    public enum IF_Marker_GroupType {
        Model,
        Effects,
        Target,
        Garbage,
        Hint
    }

    public sealed class IF_MarkerGroup {
        public readonly long? index;
        public readonly string? name;

        public readonly List<IF_MarkerGroup> markers = new();
        public readonly IF_Marker_GroupType groupType;

        public IF_MarkerGroup(TagFieldBlockElement markerGroupElement, List<IF_ArmatureNode> nodes, List<IF_ArmatureNode> regions, string markerGroupNameField = "name", string markersFieldBlock = "markers") {
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
                this.markers.Add(new IF_MarkerGroup(e, nodes, regions));
            }
        }
    }

    public sealed class IF_Marker {
        public readonly long? index;
        public readonly long? regionIndex;
        public readonly long? permutationIndex;
        public readonly long? nodeIndex;

        public readonly Position3d translation;
        public readonly Quaternion rotation;
        public readonly float? scale;
        public readonly Vector3? direction;

        public readonly bool nodeRelativePosition;

        public IF_Marker(TagFieldBlockElement markerElement, List<IF_ArmatureNode> nodes, List<IF_ArmatureNode> regions) {
            this.index = markerElement.ElementIndex;
            this.regionIndex = markerElement.SelectFieldType<TagFieldElementInteger>("region index")?.Data;
            this.permutationIndex = markerElement.SelectFieldType<TagFieldElementInteger>("permutation index")?.Data;
            this.nodeIndex = markerElement.SelectFieldType<TagFieldElementInteger>("node index")?.Data;

            this.translation = Position3d.FromTagIntArray(markerElement.SelectFieldType<TagFieldElementArrayInteger>("translation"));
            this.rotation = BlamMathematics.TagIntArrayToQuaternion(markerElement.SelectFieldType<TagFieldElementArrayInteger>("rotation"));
            this.direction = Position3d.FromTagIntArray(markerElement.SelectFieldType<TagFieldElementArrayInteger>("direction")).AsBlam;
            this.scale = markerElement.SelectFieldType<TagFieldElementInteger>("scale");

            var flags = markerElement.SelectFieldType<TagFieldFlags>("flags");
            this.nodeRelativePosition = flags.TestBit("has node relative position");
        }
    }
}