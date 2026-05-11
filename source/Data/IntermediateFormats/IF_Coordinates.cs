
using System.Numerics;
using Huragok.Utilities;

namespace Huragok.Data.IntermediateFormats.Coordinates {
    /// <summary>
    /// An <see cref="Enum"/> representing the possible coordinate unit sizes we are working with.
    /// </summary>
    internal enum IF_CoordinateUnit {
        /// <summary>
        /// The Blam native unit; exactly 3.048 meters (10 feet).
        /// </summary>
        Blam,
        /// <summary>
        /// The Jointed Model Skeleton unit; exactly 100 Blam Units, or approximately 0.031 meters.
        /// </summary>
        JMS,
        /// <summary>
        /// Metric units (base unit is meters); approximately .32 world units.
        /// </summary>
        Metric
    }

    /// <summary>
    /// <para>An intermediate <see cref="Vector3"/>-like format, used to store a 3D coordinate.</para>
    /// <para>Allows conversions to one of the available <see cref="IF_CoordinateUnit"/>s.</para>
    /// </summary>
    internal sealed class IF_RealPoint3d {
        /// <summary>
        /// Read this coordinate in the original Blam world units.
        /// </summary>
        internal Vector3 AsBlam => this.backingXYZ;
        /// <summary>
        /// Read this coordinate in JMS units.
        /// </summary>
        internal Vector3 AsJMS => this.backingXYZ * GlobalConstants.WU_TO_JMS;
        /// <summary>
        /// Read this coordinate in metric units.
        /// </summary>
        internal Vector3 AsMetric => this.backingXYZ * GlobalConstants.WU_TO_METERS;
        /// <summary>
        /// Read this coordinate in a +Z Up, +Y forward coordinate system, instead of +Z up, -X forward.
        /// </summary>
        internal IF_RealPoint3d FlipAxes => new(this.backingXYZ.Y, this.backingXYZ.Z, -this.backingXYZ.X, IF_CoordinateUnit.Blam);

        private readonly Vector3 backingXYZ;

        /// <summary>
        /// Constructs a new <see cref="IF_RealPoint3d"/> from an X, Y and Z component.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="originalSpace">
        ///     <para>The original <see cref="IF_CoordinateUnit"/> the point was in. Most commonly <see cref="IF_CoordinateUnit.Blam"/>.</para>
        ///     <para>Required to properly convert the point into other coordinate spaces.</para>
        /// </param>
        internal IF_RealPoint3d(float x, float y, float z, IF_CoordinateUnit originalSpace) {
            var tempV3 = new Vector3(x, y, z);

            switch (originalSpace) {
                case IF_CoordinateUnit.Blam:
                    this.backingXYZ = tempV3;
                    break;

                case IF_CoordinateUnit.JMS:
                    this.backingXYZ = tempV3 * GlobalConstants.JMS_TO_WU;
                    break;

                case IF_CoordinateUnit.Metric:
                    this.backingXYZ = tempV3 * GlobalConstants.METERS_TO_WU;
                    break;
            }
        }

        /// <summary>
        /// Constructs a new <see cref="IF_RealPoint3d"/> from a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="xyz">A <see cref="Vector3"/> containing the X, Y, and Z points.</param>
        /// <param name="originalSpace">
        ///     <para>The original <see cref="IF_CoordinateUnit"/> the point was in. Most commonly <see cref="IF_CoordinateUnit.Blam"/>.</para>
        ///     <para>Required to properly convert the point into other coordinate spaces.</para>
        /// </param>
        internal IF_RealPoint3d(Vector3 xyz, IF_CoordinateUnit originalSpace) {
            switch (originalSpace) {
                case IF_CoordinateUnit.Blam:
                    this.backingXYZ = xyz;
                    break;

                case IF_CoordinateUnit.JMS:
                    this.backingXYZ = xyz * GlobalConstants.JMS_TO_WU;
                    break;

                case IF_CoordinateUnit.Metric:
                    this.backingXYZ = xyz * GlobalConstants.METERS_TO_WU;
                    break;
            }
        }

        /// <summary>
        /// <para>Converts a tag float array of length 3 to a <see cref="IF_RealPoint3d"/>.</para>
        /// <para>Element 0 becomes X, 1 becomes Y, 2 becomes Z.</para>
        /// </summary>
        /// <param name="tagFloatArray">A <see cref="TagFieldElementArraySingle"/> of length 3.</param>
        /// <returns>A <see cref="IF_RealPoint3d"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static IF_RealPoint3d FromTagFloatArray(TagFieldElementArraySingle tagFloatArray) {
            float[] v3Data = tagFloatArray.Data;
            if (v3Data.Length != 3) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Vector3)}; incorrect number of elements (got {v3Data.Length}, expected 3)");
            return new IF_RealPoint3d(v3Data[0], v3Data[1], v3Data[2], IF_CoordinateUnit.Blam);
        }

        /// <param name="coordinateSpace"></param>
        /// <returns>A <see cref="Vector3"/> containing the coordinate in the supplied coordinate space.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal Vector3 ConvertToUnits(IF_CoordinateUnit coordinateSpace) {
            return coordinateSpace switch {
                IF_CoordinateUnit.Blam => this.AsBlam,
                IF_CoordinateUnit.JMS => this.AsJMS,
                IF_CoordinateUnit.Metric => this.AsMetric,
                _ => throw new InvalidOperationException($"Error in {nameof(ConvertToUnits)}: default case reached.")
            };
        }
    }

    /// <summary>
    /// <para>An intermediate <see cref="Quaternion"/>-like format, used to store a rotation.</para>
    /// </summary>
    internal sealed class IF_RealQuaterion {
        /// <summary>
        /// Read this quaternion in a +Y Up, +Z forward coordinate system, instead of +Z up, -X forward.
        /// </summary>
        internal IF_RealQuaterion FlipAxes => new(this.backingXYZW.Y, this.backingXYZW.Z, -this.backingXYZW.X, this.backingXYZW.W);
        /// <summary>
        /// <para>Value of the quaternion in the original Blam coordinate space.</para>
        /// </summary>
        internal Quaternion Value => this.backingXYZW;

        private readonly Quaternion backingXYZW;

        internal IF_RealQuaterion(float x, float y, float z, float w) {
            this.backingXYZW = new(x, y, z, w);
        }

        /// <summary>
        /// <para>Converts a tag float array of length 4 to a <see cref="IF_RealQuaterion"/>.</para>
        /// </summary>
        /// <param name="tagFloatArray">A <see cref="TagFieldElementArraySingle"/> of length 4.</param>
        /// <returns>A <see cref="IF_RealQuaterion"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static IF_RealQuaterion FromTagFloatArray(TagFieldElementArraySingle tagFloatArray) {
            float[] qData = tagFloatArray.Data;
            if (qData.Length != 4) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Quaternion)}; incorrect number of elements (got {qData.Length}, expected 4)");
            return new IF_RealQuaterion(qData[0], qData[1], qData[2], qData[3]);
        }
    }

    internal sealed class IF_RealPlane2d {
        public readonly double I;
        public readonly double J;
        public readonly double K;

        internal IF_RealPlane2d FlipAxes => new(this.J, this.K, -this.I);

        internal IF_RealPlane2d(double i, double j, double k) {
            this.I = i;
            this.J = j;
            this.K = k;
        }
    }

    internal sealed class IF_RealPlane3d {
        public readonly double I;
        public readonly double J;
        public readonly double K;
        public readonly double D;

        internal IF_RealPlane3d FlipAxes => new(this.J, this.K, -this.I, this.D);

        internal IF_RealPlane3d(double i, double j, double k, double d) {
            this.I = i;
            this.J = j;
            this.K = k;
            this.D = d;
        }
    }
}