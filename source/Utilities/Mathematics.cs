
using System.Numerics;

namespace Huragok.Utilities {
    internal static class BlamMathematics {
        internal static Quaternion TagIntArrayToQuaternion(TagFieldElementArrayInteger tagIntArray) {
            long[] qData = tagIntArray.Data;
            if (qData.Length != 4) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Vector3)}; incorrect number of elements (got {qData.Length}, expected 4)");

            return new Quaternion(qData[0], qData[1], qData[2], qData[3]);
        }

        internal static Quaternion TagFloatArrayToQuaternion(TagFieldElementArraySingle tagFloatArray) {
            float[] qData = tagFloatArray.Data;
            if (qData.Length != 4) throw new ArgumentOutOfRangeException($"Cannot cast from type {nameof(TagFieldElementArray)} to {nameof(Vector3)}; incorrect number of elements (got {qData.Length}, expected 4)");

            return new Quaternion(qData[0], qData[1], qData[2], qData[3]);
        }
    }
}