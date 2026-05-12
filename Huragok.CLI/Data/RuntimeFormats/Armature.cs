
using System.Numerics;
using Huragok.Data.Tags;

namespace Huragok.Data.RuntimeFormats;

/// <summary>
/// Intermediate representation of an armature node, or bone.
/// </summary>
internal sealed class ArmatureNode {
    internal readonly long index;
    internal readonly string? name;
    /// <summary>
    /// Parent of this node; references another node. If null, this node is the root, or is orphaned.
    /// </summary>
    internal ArmatureNode? parent;
    /// <summary>
    /// List of all nodes whose parent is this node.
    /// </summary>
    internal List<ArmatureNode>? children = new();

    /// <summary>
    /// A <see cref="Vector3"/> representing this nodes translation from its parent.
    /// </summary>
    internal readonly Vector3 defaultTranslation;
    /// <summary>
    /// A <see cref="Quaternion"/> representing the default relative rotation from the node's parent.
    /// </summary>
    internal readonly Quaternion defaultRotation;

    /// <summary>
    /// Constructs an <see cref="ArmatureNode"/> from a given <see cref="TagFieldBlockElement"/>.
    /// </summary>
    /// <param name="nodeElement">The <see cref="TagFieldBlockElement"/> representing the node.</param>
    /// <param name="nodeNameField">The name of the field containing the node's name.</param>
    internal ArmatureNode(TagFieldBlockElement nodeElement, string nodeNameField = "name") {
        this.name = nodeElement.SelectFieldType<TagFieldElementStringID>(nodeNameField).Data;
        this.index = nodeElement.ElementIndex;

        this.defaultTranslation = TagProjector.FromTagFloatArray<Vector3>(nodeElement.SelectFieldType<TagFieldElementArraySingle>("RealPoint3d:default translation"));

        this.defaultRotation = TagProjector.FromTagFloatArray<Quaternion>(nodeElement.SelectFieldType<TagFieldElementArraySingle>("default rotation"));
    }

    /// <summary>
    /// Constructs a full node graph, or armature, from a given <see cref="TagFieldBlock"/> of nodes.
    /// </summary>
    /// <param name="nodesBlock">A <see cref="TagFieldBlock"/> containing a list of nodes.</param>
    /// <param name="nodeNameField">An optional <see cref="string"/> representing the name of the field where the node's name is found.
    /// <para>Not the name of the node itself.</para>
    /// </param>
    /// <param name="parentNodeField">An optional <see cref="string"/> representing the name of the field where the node's parent is found.</param>
    /// <returns>A list of all nodes in the constructed armature -- a fully constructed armature.</returns>
    internal static List<ArmatureNode> BuildNodeGraph(TagFieldBlock nodesBlock, string nodeNameField = "name", string parentNodeField = "parent node") {
        var nodes = new List<ArmatureNode>();

        foreach (var element in nodesBlock.Elements.Cast<TagFieldBlockElement>()) {
            nodes.Add(new ArmatureNode(element, nodeNameField));

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