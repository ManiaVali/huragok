
using System.Numerics;
using Huragok.Data.IntermediateFormats.Coordinates;

// Generic Halo mesh data structures for use in decoding and converting Blam! engine mesh data.
// Constructs an intermediate format, DecodedExportGeometry, which is then parsed into an output file (OBJ, GLTF, etc).

// TODO
// - Add marker and markergroup support.
// - Water face support?
namespace Huragok.Data.IntermediateFormats.Mesh {
    public readonly struct Face {
        public readonly int T1;
        public readonly int T2;
        public readonly int T3;

        public Face(int T1, int T2, int T3) {
            this.T1 = T1;
            this.T2 = T2;
            this.T3 = T3;
        }
    }

    public sealed class IF_MeshVariant {
        public readonly int index;
        public readonly string? name;
        public readonly List<IF_MeshRegion> regions = new();
        internal readonly TagFieldBlock? regionsBlock;

        public IF_MeshVariant(TagFieldBlockElement variantBlock) {
            this.index = variantBlock.ElementIndex;
            this.name = variantBlock.SelectFieldType<TagFieldElement>("name").GetStringData();

            this.regionsBlock = variantBlock.SelectFieldType<TagFieldBlock>("regions");

            foreach (var element in this.regionsBlock.Elements.Cast<TagFieldBlockElement>()) {
                this.regions.Add(new IF_MeshRegion(element, this));
            }
        }
    }

    public sealed class IF_MeshRegion {
        public readonly int index;
        public readonly int? parentIndex;
        public readonly string? name;
        public readonly List<IF_MeshPermutation> permutations = new();
        internal readonly TagFieldBlock? permutationsBlock;

        public IF_MeshRegion(TagFieldBlockElement regionsBlock, IF_MeshVariant? variant = null) {
            this.index = regionsBlock.ElementIndex;
            this.parentIndex = variant?.index;

            // If bungie had named their fields consistently I wouldn't have to do this.
            this.name = regionsBlock.SelectFieldType<TagFieldElement>("region name")?.GetStringData();
            this.name ??= regionsBlock.SelectFieldType<TagFieldElement>("name")?.GetStringData();

            this.permutationsBlock = regionsBlock.SelectFieldType<TagFieldBlock>("permutations");

            foreach (var element in this.permutationsBlock.Elements.Cast<TagFieldBlockElement>()) {
                this.permutations?.Add(new IF_MeshPermutation(element, this));
            }
        }
    }

    public sealed class IF_MeshPermutation {
        public readonly int index;
        public readonly int? parentIndex;
        public readonly string? name;

        public readonly long meshIndex;
        public readonly long meshCount;
        internal readonly IF_MeshRegion? belongsToRegion;

        public IF_MeshPermutation(TagFieldBlockElement permutationsBlock, IF_MeshRegion region) {
            this.index = permutationsBlock.ElementIndex;
            this.parentIndex = region.index;

            // Again with the whole different freaking field names thing.
            this.name = permutationsBlock.SelectFieldType<TagFieldElement>("permutation name")?.GetStringData();
            this.name ??= permutationsBlock.SelectFieldType<TagFieldElement>("name")?.GetStringData();

            this.meshIndex = permutationsBlock.SelectFieldType<TagFieldElementInteger>("mesh index")?.Data ?? -1;
            this.meshCount = permutationsBlock.SelectFieldType<TagFieldElementInteger>("mesh count")?.Data ?? -1;
            this.belongsToRegion = region;
        }
    }

    public sealed class IF_MeshPart {
        public readonly long index;
        public readonly long materialIndex;
        public readonly long indexStart;
        public readonly long indexCount;
        internal readonly TagFieldFlags? flags;
        public readonly bool isWaterSurface;

        public IF_MeshPart(TagFieldBlockElement partElement) {
            this.index = partElement.ElementIndex;

            this.materialIndex = partElement.SelectFieldType<TagFieldBlockIndex>("render method index").Value;
            this.indexStart = partElement.SelectFieldType<TagFieldElementInteger>("index start").Data;
            this.indexCount = partElement.SelectFieldType<TagFieldElementInteger>("index count").Data;

            this.flags = partElement.SelectFieldType<TagFieldFlags>("part flags");
            this.isWaterSurface = this.flags.TestBit("is water surface");
        }
    }

    public sealed class IF_MeshSubPart {
        public readonly long index;
        public readonly long indexStart = -1;
        public readonly long indexCount = -1;
        public readonly long partIndex = -1;
        public readonly bool isWaterSubpart;
        public readonly bool isWaterSurface;
        public readonly IF_MeshPart? part;

        public IF_MeshSubPart(TagFieldBlockElement subPartElement, List<IF_MeshPart> parts, List<long>? waterIndices = null) {
            this.index = subPartElement.ElementIndex;
            this.indexStart = subPartElement.SelectFieldType<TagFieldElementInteger>("index start").Data;
            this.indexCount = subPartElement.SelectFieldType<TagFieldElementInteger>("index count").Data;
            this.partIndex = subPartElement.SelectFieldType<TagFieldBlockIndex>("part index").Value;
            this.part = parts.FirstOrDefault(p => p.index == this.partIndex);

            this.isWaterSubpart = waterIndices != null && waterIndices.Contains(this.indexStart);
            this.isWaterSurface = (this.part != null) && this.part.isWaterSurface;
        }
    }

    public sealed class IF_CompressionBounds {

        public readonly Vector3 posBounds0;
        public readonly Vector3 posBounds1;

        public readonly Vector2 uvCoords0;
        public readonly Vector2 uvCoords1;

        public IF_CompressionBounds(TagFieldBlockElement compressionBlock) {
            // The original fields here are mislabled in the engine due to "legacy reasons" and have to be remapped.

            float[] firstPosition = compressionBlock.SelectFieldType<TagFieldElementArraySingle>("position bounds 0").Data;
            float[] secondPosition = compressionBlock.SelectFieldType<TagFieldElementArraySingle>("position bounds 1").Data;

            float x0 = firstPosition[0];
            float x1 = firstPosition[1];
            float y0 = firstPosition[2];

            float y1 = secondPosition[0];
            float z0 = secondPosition[1];
            float z1 = secondPosition[2];

            this.posBounds0 = new(x0, y0, z0);
            this.posBounds1 = new(x1, y1, z1);

            float[] firstUV = compressionBlock.SelectFieldType<TagFieldElementArraySingle>("texcoord bounds 0").Data;
            float[] secondUV = compressionBlock.SelectFieldType<TagFieldElementArraySingle>("texcoord bounds 1").Data;

            this.uvCoords0 = new(firstUV[0], secondUV[0]);
            this.uvCoords1 = new(firstUV[1], secondUV[1]);
        }

        public Position3d Decompress(Vector3 compressedPosition) {
            var tmpV3 = this.posBounds0 + compressedPosition * (this.posBounds1 - this.posBounds0);
            return new Position3d(tmpV3, CoordinateUnit.Blam); // Decompress in blam space. Final scaling is done later.
        }

        public Vector2 Decompress(Vector2 compressedTexCoord) => this.uvCoords0 + compressedTexCoord * (this.uvCoords1 - this.uvCoords0);

    }

    public sealed class IF_Mesh {
        public readonly long index;
        public readonly List<IF_MeshPart> parts = new();
        public readonly List<IF_MeshSubPart> subParts = new();
        public readonly List<long>? waterIndices = new();

        public readonly bool isPCA = false;
        public readonly bool compressed = true;

        public IF_Mesh(TagFieldBlockElement meshElement, TagPath? tagPath = null) {
            this.index = meshElement.ElementIndex;

            foreach (var e in meshElement.SelectFieldType<TagFieldBlock>("water indices start").Elements.Cast<TagFieldBlockElement>()) {
                var asInteger = e.Fields[0] as TagFieldElementInteger;
                if (asInteger != null) this.waterIndices.Add(asInteger.Data);
            }

            foreach (var partElement in meshElement.SelectFieldType<TagFieldBlock>("parts").Elements.Cast<TagFieldBlockElement>()) {
                var partToAdd = new IF_MeshPart(partElement);
                this.parts.Add(partToAdd);
            }

            foreach (var subPartElement in meshElement.SelectFieldType<TagFieldBlock>("subparts").Elements.Cast<TagFieldBlockElement>()) {
                var subPartToAdd = new IF_MeshSubPart(subPartElement, this.parts, this.waterIndices);
                this.subParts.Add(subPartToAdd);
            }

#if USING_BLAM_H2AMP || USING_BLAM_H4
            var meshFlags = meshElement.SelectFieldType<TagFieldFlags>("mesh flags");
            this.isPCA = meshFlags.TestBit("mesh is PCA");
            this.compressed = !meshFlags.TestBit("use uncompressed vertex format");
#endif

        }
    }

    public sealed class IF_MeshExportGeometry {
        public readonly long meshIndex;
        public readonly IF_MeshPermutation permutation;
        public readonly List<Vector3> positions = new();
        public readonly List<Face> faces = new();
        public readonly List<long> faceMaterialIndices = new();
        public readonly List<Vector3> vtxNormals = new();
        public readonly List<Vector2> texCoords = new();
        public readonly IF_Mesh ourMesh;
        public readonly IF_CompressionBounds bounds;
        internal readonly TagPath tagPath;

        public readonly List<Vector4> nodeIndices = new();
        public readonly List<Vector4> nodeWeights = new();

        public IF_MeshExportGeometry(TagFieldBlock perMeshTempDataBlock, IF_MeshPermutation forPermutation, IF_Mesh ourMesh, IF_CompressionBounds bounds, TagPath tagPath, CoordinateUnit coordinateSpace) {
            this.meshIndex = (long)forPermutation.meshIndex;
            this.permutation = forPermutation;
            this.ourMesh = ourMesh;
            this.bounds = bounds;
            this.tagPath = tagPath;

            float[] rawPositions = GameRenderModel.GetPositionsFromMesh(perMeshTempDataBlock, (int)this.meshIndex);
            if (rawPositions.Length % 3 != 0) throw new InvalidDataException($"Error decoding {tagPath.ShortNameWithExtension}; vertex position list not divisible by 3.");
            for (int i = 0; i < rawPositions.Length; i += 3) {
                var originalPositions = bounds.Decompress(new Vector3(rawPositions[i], rawPositions[i + 1], rawPositions[i + 2]));
                var convertedSpacePositions = originalPositions.FlipAxes.ConvertToUnits(coordinateSpace);
                this.positions.Add(convertedSpacePositions);
            }

            float[] rawNorms = GameRenderModel.GetNormalsFromMesh(perMeshTempDataBlock, (int)this.meshIndex);
            if (rawNorms.Length % 3 != 0) throw new InvalidDataException($"Error decoding {tagPath.ShortNameWithExtension}; vertex normals list not divisible by 3.");
            for (int i = 0; i < rawNorms.Length; i += 3) {
                var originalNorms = new Position3d(rawNorms[i], rawNorms[i + 1], rawNorms[i + 2], CoordinateUnit.Blam);
                var convertedSpaceNorms = originalNorms.FlipAxes.ConvertToUnits(coordinateSpace);

                this.vtxNormals.Add(convertedSpaceNorms);
            }

            float[] rawCoords = GameRenderModel.GetTexCoordsFromMesh(perMeshTempDataBlock, (int)this.meshIndex);
            if (rawCoords.Length % 2 != 0) throw new InvalidDataException($"Error decoding {tagPath.ShortNameWithExtension}; texture coordinates list not divisible by 2.");
            for (int i = 0; i < rawCoords.Length; i += 2) {
                var coords = bounds.Decompress(new Vector2(rawCoords[i], rawCoords[i + 1]));
                this.texCoords.Add(coords);
            }

            byte[] rawNodeIndices = GameRenderModel.GetNodeIndiciesFromMesh(perMeshTempDataBlock, (int)this.meshIndex);
            if (rawNodeIndices.Length % 4 != 0) throw new InvalidDataException($"Error decoding {tagPath.ShortNameWithExtension}; node indices list not divisible by 4.");
            for (int i = 0; i < rawNodeIndices.Length; i += 4) {
                var indexGroup = new Vector4(rawNodeIndices[i], rawNodeIndices[i + 1], rawNodeIndices[i + 2], rawNodeIndices[i + 3]);
                this.nodeIndices.Add(indexGroup);
            }

            float[] rawNodeWeights = GameRenderModel.GetNodeWeightsFromMesh(perMeshTempDataBlock, (int)this.meshIndex);
            if (rawNodeWeights.Length % 4 != 0) throw new InvalidDataException($"Error decoding {tagPath.ShortNameWithExtension}; node indices list not divisible by 4.");
            for (int i = 0; i < rawNodeWeights.Length; i += 4) {
                var weightGroup = new Vector4(rawNodeWeights[i], rawNodeWeights[i + 1], rawNodeWeights[i + 2], rawNodeWeights[i + 3]);
                this.nodeWeights.Add(weightGroup);
            }

            // This section decodes triangle strips into triangle faces.
            // Some engine variants store tris as triangle strips instead of a list of faces.
            // Every new index after the first two forms a triangle with the previous two indices.
            // Unwinding order alternates every triangle, so we must flip the order lest the faces incorrectly alternate orientations.
            // TODO: Add support for triangle lists, as not all engine variants use strips.
            var ourPMTBlock = (TagFieldBlockElement)perMeshTempDataBlock.Elements[(int)this.meshIndex];
            var rawIndicesBlock = ourPMTBlock.SelectFieldType<TagFieldBlock>("raw indices");
            List<long> rawIndices = new();
            foreach (var index in rawIndicesBlock.Elements) {
                long word = index.SelectFieldType<TagFieldElementInteger>("word").Data;
                rawIndices.Add(word);
            }

            foreach (var part in ourMesh.parts) {
                int start = (int)part.indexStart;
                int end = start + (int)part.indexCount;

                for (int pos = start; pos < end - 2; pos++) {
                    int a = (int)rawIndices[pos];
                    int b = (int)rawIndices[pos + 1];
                    int c = (int)rawIndices[pos + 2];

                    // Skip degenerate triangles.
                    if (a == b || a == c || b == c) continue;

                    int localPos = pos - start;

                    // Unwind strips, alternating orientation each face.
                    var face = (localPos % 2 == 0)
                        ? new Face(a, b, c)
                        : new Face(a, c, b);

                    this.faces.Add(face);

                    // Assign the materials per-face from the material index of the part in question.
                    this.faceMaterialIndices.Add(part.materialIndex);
                }
            }

            // // convert unpacked indices into faces
            // List<int> unpacked = new();
            // for (int i = 0; i < unpacked.Count; i += 3) {
            //     this.faces.Add(new Face(
            //         unpacked[i],
            //         unpacked[i + 1],
            //         unpacked[i + 2]
            //     ));
            // }
        }
    }

    // Will be used later.
    // public sealed class InstancePlacement {
    //     public readonly string name = string.Empty;
    //     public readonly long index = -1;
    //     public readonly float scale = 1;
    //     public readonly long nodeIndex = -1;
    //     public readonly Vector3 forwardVector = new();
    //     public readonly Vector3 leftVector = new();
    //     public readonly Vector3 upVector = new();
    //     public readonly Vector3 position = new();

    //     public InstancePlacement(TagFieldBlockElement instancesBlock, List<Node> nodes) {
    //         this.name = instancesBlock.SelectFieldType<TagFieldElement>("name").GetStringData();
    //         if (string.IsNullOrEmpty(this.name)) this.name = "__";

    //         this.index = instancesBlock.ElementIndex;
    //         this.scale = instancesBlock.SelectFieldType<TagFieldElementInteger>("scale").Data;
    //         this.nodeIndex = instancesBlock.SelectFieldType<TagFieldElementInteger>("node index").Data;
    //         this.forwardVector = BlamMathematics.TagIntArrayToVector3(instancesBlock.SelectFieldType<TagFieldElementArrayInteger>("position"));
    //         this.leftVector = BlamMathematics.TagIntArrayToVector3(instancesBlock.SelectFieldType<TagFieldElementArrayInteger>("left"));
    //         this.upVector = BlamMathematics.TagIntArrayToVector3(instancesBlock.SelectFieldType<TagFieldElementArrayInteger>("up"));
    //         this.position = BlamMathematics.TagIntArrayToVector3(instancesBlock.SelectFieldType<TagFieldElementArrayInteger>("position")) * GlobalConstants.WU_TO_METERS;
    //     }
    // }
}