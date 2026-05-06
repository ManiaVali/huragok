using System.Drawing;

namespace Huragok.Utilities.Imaging {
    internal enum CubemapFace {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }

    internal struct CubemapFaces {
        internal Bitmap PX, NX, PY, NY, PZ, NZ;
    }

    internal enum CubemapFormat {
        Raw,
        Equirectangular
    }

    internal static class CubemapTools {
        internal static Bitmap ToEquirectangular(Bitmap bitmapReference) {
            var faces = ExtractFaces(bitmapReference);
            int faceSize = faces.PX.Width;

            int width = faceSize * 4;
            int height = faceSize * 2;

            var output = new Bitmap(width, height);

            for (int y = 0; y < height; y++) {
                float v = 1f - ((float)y / height);
                float phi = (v - .5f) * MathF.PI;

                for (int x = 0; x < width; x++) {
                    float u = (float)x / width;
                    float theta = (u - .5f) * 2f * MathF.PI;

                    float dx = MathF.Cos(phi) * MathF.Cos(theta);
                    float dy = MathF.Sin(phi);
                    float dz = MathF.Cos(phi) * MathF.Sin(theta);

                    var c = Sample(faces, dx, dy, dz, faceSize);
                    output.SetPixel(x, y, c);
                }
            }

            return output;
        }

        private static Color Sample(CubemapFaces faces, float x, float y, float z, int size) {
            float ax = Math.Abs(x);
            float ay = Math.Abs(y);
            float az = Math.Abs(z);

            CubemapFace face;
            float u, v;

            if (ay >= ax && ay >= az) {
                if (y > 0) {
                    face = CubemapFace.PositiveY;
                    u = (x / ay + 1f) * 0.5f;
                    v = (z / ay + 1f) * 0.5f;
                } else {
                    face = CubemapFace.NegativeY;
                    u = (x / ay + 1f) * 0.5f;
                    v = (-z / ay + 1f) * 0.5f;
                }
            } else if (ax >= ay && ax >= az) {
                if (x > 0) {
                    face = CubemapFace.PositiveX;
                    u = (-z / ax + 1f) * 0.5f;
                    v = (-y / ax + 1f) * 0.5f;
                } else {
                    face = CubemapFace.NegativeX;
                    u = (z / ax + 1f) * 0.5f;
                    v = (-y / ax + 1f) * 0.5f;
                }
            } else {
                if (z > 0) {
                    face = CubemapFace.PositiveZ;
                    u = (x / az + 1f) * 0.5f;
                    v = (-y / az + 1f) * 0.5f;
                } else {
                    face = CubemapFace.NegativeZ;
                    u = (-x / az + 1f) * 0.5f;
                    v = (-y / az + 1f) * 0.5f;
                }
            }

            var faceBmp = face switch {
                CubemapFace.PositiveX => faces.PX,
                CubemapFace.NegativeX => faces.NX,
                CubemapFace.PositiveY => faces.PY,
                CubemapFace.NegativeY => faces.NY,
                CubemapFace.PositiveZ => faces.PZ,
                CubemapFace.NegativeZ => faces.NZ,
                _ => throw new Exception()
            };

            int px = Math.Clamp((int)(u * (size - 1)), 0, size - 1);
            int py = Math.Clamp((int)(v * (size - 1)), 0, size - 1);

            return faceBmp.GetPixel(px, py);
        }

        private static CubemapFaces ExtractFaces(Bitmap bmp) {
            int f = bmp.Height / 3;

            Rectangle Rect(int x, int y) => new Rectangle(x, y, f, f);

            return new CubemapFaces {
                PY = bmp.Clone(Rect(0, 0), bmp.PixelFormat),
                NY = bmp.Clone(Rect(0, 2 * f), bmp.PixelFormat),

                PX = bmp.Clone(Rect(f, f), bmp.PixelFormat),
                NX = bmp.Clone(Rect(3 * f, f), bmp.PixelFormat),

                PZ = bmp.Clone(Rect(0, f), bmp.PixelFormat),
                NZ = bmp.Clone(Rect(2 * f, f), bmp.PixelFormat),
            };
        }
    }
}