using System.Numerics;

namespace Huragok.Utilities {
    internal static class BlamMathematics {

        internal static T FromTagFloatArray<T>(TagFieldElementArraySingle floats)
            where T : struct {
            float[] data = floats.Data;

            object result = typeof(T) switch {
                var t when t == typeof(Vector2) => CreateVector2(data, floats),
                var t when t == typeof(Vector3) => CreateVector3(data, floats),
                var t when t == typeof(Quaternion) => CreateQuaternion(data, floats),

                _ => throw new NotSupportedException(
                    $"Unsupported type `{typeof(T).Name}`.")
            };

            return (T)result;
        }

        private static Vector2 CreateVector2(
            float[] data,
            TagFieldElementArraySingle floats) {
            if (data.Length != 2) {
                throw new ArgumentException($"Cannot create {nameof(Vector2)} from field `{floats.FieldName}`.");
            }

            return new Vector2(data[0], data[1]);
        }

        private static Vector3 CreateVector3(
            float[] data,
            TagFieldElementArraySingle floats) {
            if (data.Length != 3) {
                throw new ArgumentException($"Cannot create {nameof(Vector3)} from field `{floats.FieldName}`.");
            }

            return new Vector3(data[0], data[1], data[2]);
        }

        private static Quaternion CreateQuaternion(
            float[] data,
            TagFieldElementArraySingle floats) {
            if (data.Length != 4) {
                throw new ArgumentException($"Cannot create {nameof(Quaternion)} from field `{floats.FieldName}`.");
            }

            return new Quaternion(data[0], data[1], data[2], data[3]);
        }
    }
}