using System.Collections.ObjectModel;
using System.Numerics;
using Huragok.Application.Commands.Export;
using Huragok.Application.Logging;
using Huragok.Data.RuntimeFormats;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using Material = Huragok.Data.RuntimeFormats.Material;

namespace Huragok.Data.Tags;

internal enum RenderModelFormat {
    /// <summary>
    /// Simple model format; supports materials, but does not support rigging or animations.
    /// <para>Built-in export format.</para>
    /// </summary>
    OBJ,

    /// <summary>
    /// Binary version of GLTF; smaller files, harder to inspect.
    /// <para>Built-in export format.</para>
    /// </summary>
    GLB,

    /// <summary>
    /// Industry standard model format. Supports materials, rigs, and animation.
    /// Exports to GLB, then converts to FBX. 
    /// <para>Relies on external program: blender</para>
    /// </summary>
    FBX
}

[Obsolete($"New version of {nameof(RenderModelTag)}.{nameof(TryExportToDisk)} has not yet been updated to handle coordinate space conversions. This must be fixed before next release.", false)]
internal sealed class RenderModelTag : BaseTag<RenderModelFormat> {
    #region Properties/Fields

    internal ModelRoot ModelData { get; private set; }
    private TagFieldBlock BlockRegions => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:regions");
    private TagFieldBlock BlockNodes => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:nodes");
    private TagFieldBlock BlockMaterials => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:materials");
    private TagFieldBlock BlockPerMeshTemp => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:per mesh temporary");

    private TagFieldBlock BlockMeshes => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:meshes");
    private TagFieldBlock BlockMarkerGroups => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:marker groups");
    private TagFieldBlock BlockCompressionInfo => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:compression info");
    protected override string TagExtension => "render_model";

    internal List<Material> allMaterials = new();
    internal List<TagPath> shaderReferences = new();
    internal List<ArmatureNode> armatureNodes = new();

    internal List<MarkerGroup> markerGroups = new();

    private readonly List<IF_Mesh> meshes = new();
    internal ReadOnlyDictionary<int, IF_Mesh> MeshesByIndex => new(this.meshes.ToDictionary(k => k.index));

    private readonly List<IF_MeshRegion> regions = new();
    internal ReadOnlyDictionary<int, IF_MeshRegion> RegionsByIndex => new(this.regions.ToDictionary(k => k.index));

    private readonly Dictionary<long, MeshExportGeometry> exportGeometries = new();

    internal CoordinateUnit distanceUnits;
    #endregion

    #region Model Decoding
    internal RenderModelTag(TagPath renderModelTagPath, CoordinateUnit usingUnits = CoordinateUnit.Metric) : base(renderModelTagPath) {
        this.ValidateTag();

        this.distanceUnits = usingUnits;

        this.LoadModelData();
        this.ModelData = this.DumpGLTF();
    }

    private void LoadModelData() {
        Logger.Debug($"{this.TagName}: {nameof(LoadModelData)}: Reading model data ...");
        try {
            this.ReadRegions();
            this.ReadMeshes();
            this.ReadMaterials();
            this.DecodeGeometry();
            this.ReadArmature();
            this.ReadMarkers();
        } catch (Exception e) {
            throw new Exception($"RenderModel decoding failed during {nameof(LoadModelData)} stage.", e);
        }
    }

    private void ReadRegions() {
        foreach (var e in this.BlockRegions.Elements.Cast<TagFieldBlockElement>()) {
            this.regions.Add(new IF_MeshRegion(e));
        }

        Logger.Debug($"{this.TagName}: {nameof(ReadRegions)}: Decoded {this.regions.Count} region(s).");
    }

    private void ReadMarkers() {
        foreach (var e in this.BlockMarkerGroups.Elements.Cast<TagFieldBlockElement>()) {
            this.markerGroups.Add(new MarkerGroup(e));
        }
    }

    private void DecodeGeometry() {
        foreach (var region in this.RegionsByIndex.Values) {
            foreach (var perm in region.permutations) {
                var compBoundsBlock = this.BlockCompressionInfo.Elements[0];
                var bounds = new IF_CompressionBounds((TagFieldBlockElement)compBoundsBlock);

                if (perm.meshIndex >= 0 && !this.MeshesByIndex.ContainsKey((int)perm.meshIndex)) {
                    Logger.Warning($"{this.sourceTag.Path.ShortNameWithExtension}: {nameof(DecodeGeometry)}: Missing mesh index on region `{region.name}`, permutation `{perm.name}`!");
                    continue;
                }

                if (!this.MeshesByIndex.TryGetValue((int)perm.meshIndex, out var mesh)) {
                    if (perm.meshIndex != -1) { // Silently ignore -1, this is used intentionally as a no-op value by Bungie.
                        Logger.Warning($"{this.sourceTag.Path.ShortNameWithExtension}: {nameof(DecodeGeometry)}: Invalid mesh index `{perm.meshIndex}` on region `{region.name}` permutation `{perm.name}`!");
                    }
                    continue;
                }

                this.exportGeometries[perm.meshIndex] = new MeshExportGeometry(this.BlockPerMeshTemp, perm, mesh, bounds, this.sourceTag.Path);
            }
        }

        Logger.Debug($"{this.TagName}: {nameof(DecodeGeometry)}: Geometry decode complete.");
    }

    private void ReadArmature() {
        this.armatureNodes = ArmatureNode.BuildNodeGraph(this.BlockNodes);
        Logger.Debug($"{this.TagName}: {nameof(ReadArmature)}: Node graph constructed.");
    }

    private void ReadMeshes() {
        foreach (var e in this.BlockMeshes) {
            this.meshes.Add(new IF_Mesh((TagFieldBlockElement)e));
        }

        Logger.Debug($"{this.TagName}: {nameof(ReadMeshes)}: Decoded {this.meshes.Count} meshes.");
    }

    private void ReadMaterials() {
        this.allMaterials.Clear();
        this.shaderReferences.Clear();

        foreach (var mat in this.BlockMaterials.Cast<TagFieldBlockElement>()) {
            var material = new Material(mat);
            this.allMaterials.Add(material);
            this.shaderReferences.Add(material.renderMethodPath);
        }

        Logger.Debug($"{this.TagName}: {nameof(ReadMaterials)}: Render Model has {this.allMaterials.Count} materials and {this.shaderReferences.Count} shader references.");
    }
    #endregion

    #region Export Funcs
    internal override bool TryExportToDisk(string outputDirectory, RenderModelFormat fileExtension, out List<string> finalFileLocations) {
        Logger.Debug($"{this.TagName}: Disk export requested.");

        string extension = fileExtension.ToString().ToLower();
        string finalFileLocation = this.BuildOutputPath(outputDirectory, extension);

        string? outDir = Path.GetDirectoryName(finalFileLocation);
        if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

        switch (fileExtension) {
            case RenderModelFormat.OBJ:
                Logger.Warning("Writing output file as OBJ. You will lose armature rigging with this format.");
                this.ModelData.SaveAsWavefront(finalFileLocation);
                break;
            case RenderModelFormat.GLB:
                this.ModelData.SaveGLB(finalFileLocation);
                break;
            case RenderModelFormat.FBX:
                this.ModelData.SaveFBX(finalFileLocation);
                break;
            default:
                Logger.Warning($"Unexpected export format `{fileExtension}`; defaulting to GLB.");
                this.ModelData.SaveGLB(finalFileLocation);
                break;
        }

        finalFileLocations = [finalFileLocation];
        return true;
    }
    #endregion

    #region GLTF Construction
    private ModelRoot DumpGLTF() {
        var scene = new SceneBuilder();

        var basisCorrectionRoot = new NodeBuilder($"root:{this.TagNameNoExtension}");
        var basis = Matrix4x4.CreateRotationZ(MathF.PI * 0.5f) * Matrix4x4.CreateRotationX(-MathF.PI * 0.5f);

        basisCorrectionRoot.SetLocalTransform(
            new AffineTransform(
                Vector3.One,
                Quaternion.CreateFromRotationMatrix(basis),
                Vector3.Zero
            ), true
        );

        // Create all materials
        var materialMap = new Dictionary<int, MaterialBuilder>();
        for (int i = 0; i < this.allMaterials.Count; i++) {
            var mat = this.allMaterials[i];

            var gltfMat = new MaterialBuilder(mat.renderMethodPath.ShortName)
                .WithDoubleSide(true)
                .WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 1, 1, 1));

            materialMap[i] = gltfMat;
        }
        Logger.Debug($"{this.TagName}: {nameof(DumpGLTF)}: Created {materialMap.Count} materials.");

        // Build the skeleton
        var skeletonRootNode = new NodeBuilder($"armature:{this.TagNameNoExtension}");
        basisCorrectionRoot.AddNode(skeletonRootNode);

        scene.AddNode(basisCorrectionRoot);

        var gltfNodes = new Dictionary<int, NodeBuilder>();
        foreach (var node in this.armatureNodes) {
            var gltfNode = new NodeBuilder($"bone:{node.name}");
            gltfNode.SetLocalTransform(
                new AffineTransform(
                    Vector3.One,
                    node.defaultRotation,
                    node.defaultTranslation
                ), true);

            gltfNodes[(int)node.index] = gltfNode;
        }

        foreach (var node in this.armatureNodes) {
            var gltfNode = gltfNodes[(int)node.index];

            if (node.parent == null) {
                skeletonRootNode.AddNode(gltfNode);
            } else {
                gltfNodes[(int)node.parent.index].AddNode(gltfNode);
            }
        }
        Logger.Debug($"{this.TagName}: {nameof(DumpGLTF)}: Armature constructed; {gltfNodes.Count} nodes.");

        // Build meshes and set up skin weights
        long currentMaterialIndex = -1;
        foreach (var region in this.RegionsByIndex.Values) {
            foreach (var perm in region.permutations) {
                if (perm.meshIndex < 0) continue;
                if (!this.exportGeometries.TryGetValue(perm.meshIndex, out var exGeom)) continue;

                var mesh = new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>($"mesh:{region.name}:{perm.name}");
                var prim = mesh.UsePrimitive(MaterialBuilder.CreateDefault());

                for (int i = 0; i < exGeom.faces.Count; i++) {
                    var face = exGeom.faces[i];
                    long matIndex = exGeom.faceMaterialIndices[i];

                    if (currentMaterialIndex != matIndex) {
                        prim = mesh.UsePrimitive(materialMap[(int)matIndex]);
                        currentMaterialIndex = matIndex;
                    }

                    // Create vertices and add normals
                    var vp1 = new VertexPositionNormal(exGeom.positions[face.Vertex1Index], exGeom.vtxNormals[face.Vertex1Index]);
                    var vp2 = new VertexPositionNormal(exGeom.positions[face.Vertex2Index], exGeom.vtxNormals[face.Vertex2Index]);
                    var vp3 = new VertexPositionNormal(exGeom.positions[face.Vertex3Index], exGeom.vtxNormals[face.Vertex3Index]);

                    // Add texture coordinates
                    var vt1 = exGeom.texCoords[face.Vertex1Index];
                    var vt2 = exGeom.texCoords[face.Vertex2Index];
                    var vt3 = exGeom.texCoords[face.Vertex3Index];

                    // Create vertex colors
                    var vc1 = new Vector4(exGeom.vtxColors[face.Vertex1Index].X, exGeom.vtxColors[face.Vertex1Index].Y, exGeom.vtxColors[face.Vertex1Index].Z, 1);
                    var vc2 = new Vector4(exGeom.vtxColors[face.Vertex2Index].X, exGeom.vtxColors[face.Vertex2Index].Y, exGeom.vtxColors[face.Vertex2Index].Z, 1);
                    var vc3 = new Vector4(exGeom.vtxColors[face.Vertex3Index].X, exGeom.vtxColors[face.Vertex3Index].Y, exGeom.vtxColors[face.Vertex3Index].Z, 1);

                    // Combine texcoords and colors
                    var vm1 = new VertexColor1Texture1(vc1, vt1);
                    var vm2 = new VertexColor1Texture1(vc2, vt2);
                    var vm3 = new VertexColor1Texture1(vc3, vt3);

                    // Set up joints
                    var j1 = exGeom.nodeIndices[face.Vertex1Index];
                    var j2 = exGeom.nodeIndices[face.Vertex2Index];
                    var j3 = exGeom.nodeIndices[face.Vertex3Index];

                    // Set up weights
                    var w1 = exGeom.nodeWeights[face.Vertex1Index];
                    var w2 = exGeom.nodeWeights[face.Vertex2Index];
                    var w3 = exGeom.nodeWeights[face.Vertex3Index];

                    // Construct the skins
                    var skin1 = new VertexJoints4(
                        ((int)exGeom.nodeIndices[face.Vertex1Index].X, exGeom.nodeWeights[face.Vertex1Index].X),
                        ((int)exGeom.nodeIndices[face.Vertex1Index].Y, exGeom.nodeWeights[face.Vertex1Index].Y),
                        ((int)exGeom.nodeIndices[face.Vertex1Index].Z, exGeom.nodeWeights[face.Vertex1Index].Z),
                        ((int)exGeom.nodeIndices[face.Vertex1Index].W, exGeom.nodeWeights[face.Vertex1Index].W)
                    );
                    var skin2 = new VertexJoints4(
                        ((int)exGeom.nodeIndices[face.Vertex2Index].X, exGeom.nodeWeights[face.Vertex2Index].X),
                        ((int)exGeom.nodeIndices[face.Vertex2Index].Y, exGeom.nodeWeights[face.Vertex2Index].Y),
                        ((int)exGeom.nodeIndices[face.Vertex2Index].Z, exGeom.nodeWeights[face.Vertex2Index].Z),
                        ((int)exGeom.nodeIndices[face.Vertex2Index].W, exGeom.nodeWeights[face.Vertex2Index].W)
                    );
                    var skin3 = new VertexJoints4(
                        ((int)exGeom.nodeIndices[face.Vertex3Index].X, exGeom.nodeWeights[face.Vertex3Index].X),
                        ((int)exGeom.nodeIndices[face.Vertex3Index].Y, exGeom.nodeWeights[face.Vertex3Index].Y),
                        ((int)exGeom.nodeIndices[face.Vertex3Index].Z, exGeom.nodeWeights[face.Vertex3Index].Z),
                        ((int)exGeom.nodeIndices[face.Vertex3Index].W, exGeom.nodeWeights[face.Vertex3Index].W)
                    );

                    // Construct the final vertices
                    var vOut1 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(vp1, vm1, skin1);
                    var vOut2 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(vp2, vm2, skin2);
                    var vOut3 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(vp3, vm3, skin3);

                    // Add to primitive
                    prim.AddTriangle(vOut1, vOut2, vOut3);
                }

                // Construct the inverse bind matrix for armature support
                // Oh my God, I hate this code.
                Dictionary<ArmatureNode, Matrix4x4> globalMatrices = new();
                Matrix4x4 ComputeGlobal(ArmatureNode node) {
                    var local =
                        Matrix4x4.CreateFromQuaternion(node.defaultRotation) *
                        Matrix4x4.CreateTranslation(node.defaultTranslation);

                    if (node.parent == null) return local;

                    // Do not flip the multiplication order again. This causes some meshes to get displaced.
                    return local * globalMatrices[node.parent];
                }

                foreach (var node in this.armatureNodes) {
                    globalMatrices[node] = ComputeGlobal(node);
                }

                Dictionary<ArmatureNode, Matrix4x4> inverseBind = new();
                foreach (var node in this.armatureNodes) {
                    Matrix4x4.Invert(globalMatrices[node], out var inv);
                    inverseBind[node] = inv;
                }

                var joints = this.armatureNodes
                    .Select(n => (gltfNodes[(int)n.index], inverseBind[n]))
                    .ToArray();

                scene.AddSkinnedMesh(mesh, joints);
            }
        }
        Logger.Debug($"{this.TagName}: {nameof(DumpGLTF)}: {this.meshes.Count} Meshes constructed.");

        foreach (var markerGroup in this.markerGroups) {
            var markers = markerGroup.markers;

            foreach (var marker in markers) {
                var gltfNode = gltfNodes[(int)marker.nodeIndex];
                var markerNode = new NodeBuilder($"marker:{markerGroup.name}:{marker.index}");

                var markerScale = new Vector3(marker.scale, marker.scale, marker.scale);

                markerNode.SetLocalTransform(
                    new AffineTransform(
                        markerScale,
                        marker.rotation,
                        marker.translation
                    ), true
                );

                gltfNode.AddNode(markerNode);
            }
        }

        Logger.Debug($"{this.TagName}: {nameof(DumpGLTF)}: GLTF model construction completed.");
        return scene.ToGltf2();
    }
    #endregion
}