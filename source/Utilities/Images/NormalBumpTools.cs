using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Huragok.Utilities.Imaging {
    public static class NormalBumpTools {

        public static Bitmap ProcessNormalMap(Bitmap source, bool reconstructZ = true, bool swizzleGreen = false) {
            if (!reconstructZ && !swizzleGreen) return source;
            
            var bmp = source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format32bppArgb);

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var data = bmp.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );

            int bytes = Math.Abs(data.Stride) * bmp.Height;
            byte[] buffer = new byte[bytes];

            Marshal.Copy(data.Scan0, buffer, 0, bytes);

            for (int y = 0; y < bmp.Height; y++) {
                for (int x = 0; x < bmp.Width; x++) {
                    int i = y * data.Stride + x * 4;

                    // ARGB layout, BGRA in mem
                    // Also convert to gamma because that fixes it for some reason.
                    float b = GammaToLinear(buffer[i + 0] / 255f);
                    float g = GammaToLinear(buffer[i + 1] / 255f);
                    float r = GammaToLinear(buffer[i + 2] / 255f);

                    float outR;
                    float outG;
                    float outB;

                    if (reconstructZ) {
                        // Convert to -1,1 space
                        float nx = r * 2f - 1f;
                        float ny = g * 2f - 1f;

                        // Reconstruct Z channel from XY
                        float nz = MathF.Sqrt(MathF.Max(0f, 1f - nx * nx - ny * ny));

                        // Back to 0,1
                        outR = nx * .5f + .5f;
                        outG = ny * .5f + .5f;
                        outB = nz * .5f + .5f;
                    } else {
                        outR = r;
                        outG = g;
                        outB = b;
                    }

                    if (swizzleGreen) {
                        outG = 1f - outG;
                    }

                    // Write back out BGRA
                    buffer[i + 0] = (byte)(outB * 255f);    // B
                    buffer[i + 1] = (byte)(outG * 255f);    // G
                    buffer[i + 2] = (byte)(outR * 255f);    // R
                    buffer[i + 3] = 255;                    // A
                }
            }

            Marshal.Copy(buffer, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);

            return bmp;
        }

        private static float GammaToLinear(float c) => MathF.Pow(c, 2.2f);
        private static float LinearToGamma(float c) => MathF.Pow(c, 1 / 2.2f);
    }
}