
using System.Numerics;
using Huragok.Data.IntermediateFormats.Coordinates;
using Huragok.Utilities;

namespace Huragok.Data.IntermediateFormats.Armature {
    /// <summary>
    /// Intermediate representation of an armature node, or bone.
    /// </summary>
    internal sealed class IF_ArmatureNode {
        internal readonly long index;
        internal readonly string? name;
        /// <summary>
        /// Parent of this node; references another node. If null, this node is the root, or is orphaned.
        /// </summary>
        internal IF_ArmatureNode? parent;
        /// <summary>
        /// List of all nodes whose parent is this node.
        /// </summary>
        internal List<IF_ArmatureNode>? children = new();

        /// <summary>
        /// A <see cref="RealPoint3d"/> representing this nodes translation from its parent.
        /// </summary>
        internal readonly RealPoint3d defaultTranslation;
        /// <summary>
        /// A <see cref="Quaternion"/> representing the default relative rotation from the node's parent.
        /// </summary>
        internal readonly Quaternion defaultRotation;
        internal readonly Vector3 inverseForward;
        internal readonly Vector3 inverseLeft;
        internal readonly Vector3 inverseUp;
        internal readonly Vector3 inversePosition;

        internal readonly float inverseScale = 1;

        internal IF_ArmatureNode(TagFieldBlockElement nodeElement, string nodeNameField = "name") {
            this.name = nodeElement.SelectFieldType<TagFieldElement>(nodeNameField).GetStringData();
            this.index = nodeElement.ElementIndex;

            this.defaultTranslation = RealPoint3d.FromTagFloatArray(nodeElement.SelectFieldType<TagFieldElementArraySingle>("RealPoint3d:default translation"));

            this.defaultRotation = BlamMathematics.TagFloatArrayToQuaternion(nodeElement.SelectFieldType<TagFieldElementArraySingle>("default rotation"));
            this.inverseForward = RealPoint3d.FromTagFloatArray(nodeElement.SelectFieldType<TagFieldElementArraySingle>("inverse forward")).AsBlam;
            this.inverseLeft = RealPoint3d.FromTagFloatArray(nodeElement.SelectFieldType<TagFieldElementArraySingle>("inverse left")).AsBlam;
            this.inverseUp = RealPoint3d.FromTagFloatArray(nodeElement.SelectFieldType<TagFieldElementArraySingle>("inverse up")).AsBlam;
            this.inversePosition = RealPoint3d.FromTagFloatArray(nodeElement.SelectFieldType<TagFieldElementArraySingle>("inverse position")).AsBlam;
            this.inverseScale = nodeElement.SelectFieldType<TagFieldElementSingle>("inverse scale");
        }

        /// <summary>
        /// Constructs a full node graph, or armature, from a given <see cref="TagFieldBlock"/> of nodes.
        /// </summary>
        /// <param name="nodesBlock">A <see cref="TagFieldBlock"/> containing a list of nodes.</param>
        /// <param name="nodeNameField">An optional <see cref="string"/> representing the name of the field where the node's name is found.
        ///     <para>Not the name of the node itself.</para>
        /// </param>
        /// <param name="parentNodeField">An optional <see cref="string"/> representing the name of the field where the node's parent is found.</param>
        /// <returns>A list of all nodes in the constructed armature -- a fully constructed armature.</returns>
        internal static List<IF_ArmatureNode> BuildNodeGraph(TagFieldBlock nodesBlock, string nodeNameField = "name", string parentNodeField = "parent node") {
            var nodes = new List<IF_ArmatureNode>();

            foreach (var element in nodesBlock.Elements.Cast<TagFieldBlockElement>()) {
                nodes.Add(new IF_ArmatureNode(element, nodeNameField));

                int index = element.ElementIndex;
                var node = nodes[index];

                int parentIndex = element.SelectFieldType<TagFieldBlockIndex>(parentNodeField).Value;

                if (parentIndex < 0) continue;

                node.parent = nodes[parentIndex];
                nodes[parentIndex]?.children?.Add(node);

            }
            return nodes;
        }
    }
}