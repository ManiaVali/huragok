
using System.Numerics;
using Huragok.Utilities;

namespace Huragok.Data.IntermediateFormats.Coordinates {
    /// <summary>
    /// An <see cref="Enum"/> representing the possible coordinate unit sizes we are working with.
    /// </summary>
    internal enum CoordinateUnit {
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
    /// <para>Allows conversions to one of the available <see cref="CoordinateUnit"/>s.</para>
    /// </summary>
    internal sealed class RealPoint3d {
        /// <summary>
        /// Read this world coordinate in the original Blam world units.
        /// </summary>
        internal Vector3 AsBlam => this.backingXYZ;
        /// <summary>
        /// Read this world coordinate in JMS units.
        /// </summary>
        internal Vector3 AsJMS => this.backingXYZ * GlobalConstants.WU_TO_JMS;
        /// <summary>
        /// Read this world coordinate in metric units.
        /// </summary>
        internal Vector3 AsMetric => this.backingXYZ * GlobalConstants.WU_TO_METERS;
        /// <summary>
        /// Read this world coordinate in a -Y forward coordinate system, instead of -X forward.
        /// </summary>
        internal RealPoint3d FlipAxes => new(this.backingXYZ.Y, this.backingXYZ.Z, this.backingXYZ.X, CoordinateUnit.Blam);

        private readonly Vector3 backingXYZ;

        /// <summary>
        /// Constructs a new <see cref="RealPoint3d"/> from an X, Y and Z component.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="originalSpace">
        ///     <para>The original <see cref="CoordinateUnit"/> the point was in. Most commonly <see cref="CoordinateUnit.Blam"/>.</para>
        ///     <para>Required to properly convert the point into other coordinate spaces.</para>
        /// </param>
        internal RealPoint3d(float x, float y, float z, CoordinateUnit originalSpace) {
            var tempV3 = new Vector3(x, y, z);

            switch (originalSpace) {
                case CoordinateUnit.Blam:
                    this.backingXYZ = tempV3;
                    break;

                case CoordinateUnit.JMS:
                    this.backingXYZ = tempV3 * GlobalConstants.JMS_TO_WU;
                    break;

                case CoordinateUnit.Metric:
                    this.backingXYZ = tempV3 * GlobalConstants.METERS_TO_WU;
                    break;
            }
        }

        /// <summary>
        /// Constructs a new <see cref="RealPoint3d"/> from a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="xyz">A <see cref="Vector3"/> containing the X, Y, and Z points.</param>
        /// <param name="originalSpace">
        ///     <para>The original <see cref="CoordinateUnit"/> the point was in. Most commonly <see cref="CoordinateUnit.Blam"/>.</para>
        ///     <para>Required to properly convert the point into other coordinate spaces.</para>
        /// </param>
        internal RealPoint3d(Vector3 xyz, CoordinateUnit originalSpace) {
            switch (originalSpace) {
                case CoordinateUnit.Blam:
                    this.backingXYZ = xyz;
                    break;

                case CoordinateUnit.JMS:
                    this.backingXYZ = xyz * GlobalConstants.JMS_TO_WU;
                    break;

                case CoordinateUnit.Metric:
                    this.backingXYZ = xyz * GlobalConstants.METERS_TO_WU;
                    break;
            }
        }

        /// <summary>
        /// <para>Converts a tag integer array of length 3 to a <see cref="RealPoint3d"/>.</para>
        /// <para>Element 0 becomes X, 1 becomes Y, 2 becomes Z.</para>
        /// </summary>
        /// <param name="tagIntArray">A <see cref="TagFieldElementArrayInteger"/> of length 3.</param>
        /// <returns>A <see cref="RealPoint3d"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static RealPoint3d FromTagIntArray(TagFieldElementArrayInteger tagIntArray) {
            long[] v3Data = tagIntArray.Data;
            if (v3Data.Length != 3) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Vector3)}; incorrect number of elements (got {v3Data.Length}, expected 3)");
            return new RealPoint3d(v3Data[0], v3Data[1], v3Data[2], CoordinateUnit.Blam); ;
        }

        /// <summary>
        /// <para>Converts a tag float array of length 3 to a <see cref="RealPoint3d"/>.</para>
        /// <para>Element 0 becomes X, 1 becomes Y, 2 becomes Z.</para>
        /// </summary>
        /// <param name="tagFloatArray">A <see cref="TagFieldElementArraySingle"/> of length 3.</param>
        /// <returns>A <see cref="RealPoint3d"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static RealPoint3d FromTagFloatArray(TagFieldElementArraySingle tagFloatArray) {
            float[] v3Data = tagFloatArray.Data;
            if (v3Data.Length != 3) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Vector3)}; incorrect number of elements (got {v3Data.Length}, expected 3)");
            return new RealPoint3d(v3Data[0], v3Data[1], v3Data[2], CoordinateUnit.Blam);
        }

        /// <param name="coordinateSpace"></param>
        /// <returns>A <see cref="Vector3"/> containing the coordinate in the supplied coordinate space.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal Vector3 ConvertToUnits(CoordinateUnit coordinateSpace) {
            return coordinateSpace switch {
                CoordinateUnit.Blam => this.AsBlam,
                CoordinateUnit.JMS => this.AsJMS,
                CoordinateUnit.Metric => this.AsMetric,
                _ => throw new InvalidOperationException($"Error in {nameof(ConvertToUnits)}: default case reached.")
            };
        }
    }

    internal sealed class RealPlane2d {
        public readonly double I;
        public readonly double J;
        public readonly double K;

        internal RealPlane2d(double i, double j, double k) {
            this.I = i;
            this.J = j;
            this.K = k;
        }
    }

    internal sealed class RealPlane3d {
        public readonly double I;
        public readonly double J;
        public readonly double K;
        public readonly double D;

        internal RealPlane3d(double i, double j, double k, double d) {
            this.I = i;
            this.J = j;
            this.K = k;
            this.D = d;
        }
    }
}