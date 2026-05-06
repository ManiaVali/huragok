using System.Numerics;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using Huragok.Data.IntermediateFormats.Armature;
using Huragok.Data.IntermediateFormats.Coordinates;
using Huragok.Data.IntermediateFormats.Materials;
using Huragok.Data.IntermediateFormats.Mesh;
using Huragok.Commands.RenderModel;

namespace Huragok.Data.Tags {
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

    internal sealed class RenderModelTag : BaseTag<RenderModelFormat> {
        #region Properties/Fields

        internal ModelRoot ModelData { get; private set; }
        private TagFieldBlock BlockRegions => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:regions");
        private TagFieldBlock BlockNodes => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:nodes");
        private TagFieldBlock BlockMaterials => this.sourceTag.SelectFieldType<TagFieldBlock>("Block:materials");
        private TagFieldBlock BlockPerMeshTemp => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:per mesh temporary");

        private TagFieldBlock BlockMeshes => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:meshes");
        private TagFieldBlock BlockCompressionInfo => this.sourceTag.SelectFieldType<TagFieldBlock>("Struct:render geometry[0]/Block:compression info");
        protected override string TagExtension => "render_model";

        internal List<IF_Material> allMaterials = new();
        internal List<TagPath> shaderReferences = new();
        internal List<IF_ArmatureNode> nodes = new();

        private Dictionary<int, IF_Mesh> meshesByIndex;
        private Dictionary<int, IF_MeshRegion> regionsByIndex;

        private readonly Dictionary<long, IF_MeshExportGeometry> exportGeometries = new();

        internal CoordinateUnit distanceUnits;
        #endregion

        #region Model Decoding
        internal RenderModelTag(TagPath renderModelTagPath, CoordinateUnit usingUnits = CoordinateUnit.Metric) : base(renderModelTagPath) {
            this.ValidateTag();

            this.distanceUnits = usingUnits;

            this.LoadModelData();
            this.ModelData = this.DumpGLTF();

            this.meshesByIndex = new();
            this.regionsByIndex = new();
        }

        private void LoadModelData() {
            try {
                this.ReadRegions();
                this.ReadMeshes();
                this.ReadMaterials();
                this.DecodeGeometry();
                this.nodes = IF_ArmatureNode.BuildNodeGraph(this.BlockNodes);
            } catch (Exception e) {
                throw new Exception($"RenderModel decoding failed during {nameof(LoadModelData)} stage.", e);
            }
        }

        private void ReadRegions() {
            List<IF_MeshRegion> regions = new();
            foreach (var e in this.BlockRegions) {
                regions.Add(new IF_MeshRegion((TagFieldBlockElement)e));
            }
            this.regionsByIndex = regions.ToDictionary(r => r.index);
        }

        private void DecodeGeometry() {
            foreach (var region in this.regionsByIndex.Values) {
                foreach (var perm in region.permutations) {
                    var compBoundsBlock = this.BlockCompressionInfo.Elements[0];
                    var bounds = new IF_CompressionBounds((TagFieldBlockElement)compBoundsBlock);

                    if (perm.meshIndex >= 0 && !this.meshesByIndex.ContainsKey((int)perm.meshIndex)) {
                        Logger.Warning($"{this.sourceTag.Path.ShortNameWithExtension}: Missing mesh index on region `{region.name}`, permutation `{perm.name}`!");
                        continue;
                    }

                    if (!this.meshesByIndex.TryGetValue((int)perm.meshIndex, out var mesh)) {
                        if (perm.meshIndex != -1) { // Silently ignore -1, this is used intentionally as a no-op value by Bungie.
                            Logger.Warning($"{this.sourceTag.Path.ShortNameWithExtension}: Invalid mesh index `{perm.meshIndex}` on region `{region.name}` permutation `{perm.name}`!");
                        }
                        continue;
                    }

                    this.exportGeometries[perm.meshIndex] = new IF_MeshExportGeometry(this.BlockPerMeshTemp, perm, mesh, bounds, this.sourceTag.Path, this.distanceUnits);
                }
            }
        }

        private void ReadMeshes() {
            var compressFlags = this.BlockCompressionInfo.Elements[0].SelectFieldType<TagFieldFlags>("WordFlags:compression flags");

            List<IF_Mesh> meshes = new();
            foreach (var e in this.BlockMeshes) {
                meshes.Add(new IF_Mesh((TagFieldBlockElement)e));
            }
            this.meshesByIndex = meshes.ToDictionary(m => (int)m.index);
        }

        private void ReadMaterials() {
            foreach (var mat in this.BlockMaterials.Cast<TagFieldBlockElement>()) {
                var material = new IF_Material(mat);
                this.allMaterials.Add(material);
                this.shaderReferences.Add(material.renderMethodPath);
            }
        }
        #endregion

        #region Export Funcs
        internal override bool TryExportToDisk(string outputDirectory, RenderModelFormat fileExtension, out List<string> finalFileLocations) {
            string extension = fileExtension.ToString().ToLower();
            string finalFileLocation = this.BuildOutputPath(outputDirectory, extension);

            this.LoadModelData();

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
            var materialMap = new Dictionary<int, MaterialBuilder>();
            var scene = new SceneBuilder();

            var skeletonRootNode = new NodeBuilder($"{this.sourceTag.Path.ShortName}:armature");

            scene.AddNode(skeletonRootNode);

            // Create all materials
            for (int i = 0; i < this.allMaterials.Count; i++) {
                var mat = this.allMaterials[i];

                var gltfMat = new MaterialBuilder(mat.renderMethodPath.ShortName)
                    .WithDoubleSide(true)
                    .WithMetallicRoughnessShader()
                    .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 1, 1, 1));

                materialMap[i] = gltfMat;
            }

            // Build the skeleton
            var gltfNodes = new Dictionary<int, NodeBuilder>();
            foreach (var node in this.nodes) {
                var gltfNode = new NodeBuilder($"bone:{node.name}");
                gltfNode.SetLocalTransform(
                    new AffineTransform(
                        Vector3.One,
                        node.defaultRotation,
                        node.defaultTranslation.FlipAxes.ConvertToUnits(this.distanceUnits)
                    ), true);

                gltfNodes[(int)node.index] = gltfNode;
            }

            foreach (var node in this.nodes) {
                var gltfNode = gltfNodes[(int)node.index];

                if (node.parent == null) {
                    skeletonRootNode.AddNode(gltfNode);
                } else {
                    gltfNodes[(int)node.parent.index].AddNode(gltfNode);
                }
            }

            // Set up skin weights
            var jointNodes = new List<NodeBuilder>();
            var inverseBindMatrices = new List<Matrix4x4>();

            // Build meshes and assign materials
            long currentMaterialIndex = -1;
            foreach (var region in this.regionsByIndex.Values) {
                foreach (var perm in region.permutations) {
                    if (perm.meshIndex < 0) continue;
                    if (!this.exportGeometries.TryGetValue(perm.meshIndex, out var exGeom)) continue;

                    var mesh = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>($"mesh:{region.name}:{perm.name}");
                    var prim = mesh.UsePrimitive(MaterialBuilder.CreateDefault());

                    for (int i = 0; i < exGeom.faces.Count; i++) {
                        var face = exGeom.faces[i];
                        long matIndex = exGeom.faceMaterialIndices[i];

                        if (currentMaterialIndex != matIndex) {
                            prim = mesh.UsePrimitive(materialMap[(int)matIndex]);
                            currentMaterialIndex = matIndex;
                        }

                        // Create vertices and add normals
                        var vp1 = new VertexPositionNormal(exGeom.positions[face.T1], exGeom.vtxNormals[face.T1]);
                        var vp2 = new VertexPositionNormal(exGeom.positions[face.T2], exGeom.vtxNormals[face.T2]);
                        var vp3 = new VertexPositionNormal(exGeom.positions[face.T3], exGeom.vtxNormals[face.T3]);

                        // Add texture coordinates
                        var vt1 = new VertexTexture1(exGeom.texCoords[face.T1]);
                        var vt2 = new VertexTexture1(exGeom.texCoords[face.T2]);
                        var vt3 = new VertexTexture1(exGeom.texCoords[face.T3]);

                        // Set up joints
                        var j1 = exGeom.nodeIndices[face.T1];
                        var j2 = exGeom.nodeIndices[face.T2];
                        var j3 = exGeom.nodeIndices[face.T3];

                        // Set up weights
                        var w1 = exGeom.nodeWeights[face.T1];
                        var w2 = exGeom.nodeWeights[face.T2];
                        var w3 = exGeom.nodeWeights[face.T3];

                        // Construct the skins
                        var skin1 = new VertexJoints4(
                            ((int)exGeom.nodeIndices[face.T1].X, exGeom.nodeWeights[face.T1].X),
                            ((int)exGeom.nodeIndices[face.T1].Y, exGeom.nodeWeights[face.T1].Y),
                            ((int)exGeom.nodeIndices[face.T1].Z, exGeom.nodeWeights[face.T1].Z),
                            ((int)exGeom.nodeIndices[face.T1].W, exGeom.nodeWeights[face.T1].W)
                        );
                        var skin2 = new VertexJoints4(
                            ((int)exGeom.nodeIndices[face.T2].X, exGeom.nodeWeights[face.T2].X),
                            ((int)exGeom.nodeIndices[face.T2].Y, exGeom.nodeWeights[face.T2].Y),
                            ((int)exGeom.nodeIndices[face.T2].Z, exGeom.nodeWeights[face.T2].Z),
                            ((int)exGeom.nodeIndices[face.T2].W, exGeom.nodeWeights[face.T2].W)
                        );
                        var skin3 = new VertexJoints4(
                            ((int)exGeom.nodeIndices[face.T3].X, exGeom.nodeWeights[face.T3].X),
                            ((int)exGeom.nodeIndices[face.T3].Y, exGeom.nodeWeights[face.T3].Y),
                            ((int)exGeom.nodeIndices[face.T3].Z, exGeom.nodeWeights[face.T3].Z),
                            ((int)exGeom.nodeIndices[face.T3].W, exGeom.nodeWeights[face.T3].W)
                        );

                        // Construct the final vertices
                        var v1 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(vp1, vt1, skin1);
                        var v2 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(vp2, vt2, skin2);
                        var v3 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(vp3, vt3, skin3);

                        // Add to primitive
                        prim.AddTriangle(v1, v2, v3);
                    }

                    // Construct the inverse bind matrix for armature support
                    // Oh my God, I hate this code.
                    Dictionary<IF_ArmatureNode, Matrix4x4> globalMatrices = new();
                    Matrix4x4 ComputeGlobal(IF_ArmatureNode node) {
                        var local =
                            Matrix4x4.CreateFromQuaternion(node.defaultRotation) *
                            Matrix4x4.CreateTranslation(node.defaultTranslation.FlipAxes.ConvertToUnits(this.distanceUnits));

                        if (node.parent == null) return local;

                        // Do not flip the multiplication order again. This causes some meshes to get displaced.
                        return local * globalMatrices[node.parent];
                    }

                    foreach (var node in this.nodes) {
                        globalMatrices[node] = ComputeGlobal(node);
                    }

                    Dictionary<IF_ArmatureNode, Matrix4x4> inverseBind = new();
                    foreach (var node in this.nodes) {
                        Matrix4x4.Invert(globalMatrices[node], out var inv);
                        inverseBind[node] = inv;
                    }

                    var joints = this.nodes
                        .Select(n => (gltfNodes[(int)n.index], inverseBind[n]))
                        .ToArray();

                    scene.AddSkinnedMesh(mesh, joints);
                }
            }
            return scene.ToGltf2();
        }
        #endregion
    }
}