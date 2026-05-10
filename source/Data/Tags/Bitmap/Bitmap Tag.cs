
using System.Drawing;
using System.Drawing.Imaging;
using Huragok.Utilities.Imaging;

namespace Huragok.Data.Tags {
    internal enum BitmapFormat {
        PNG,
        JPG,
        TIFF,
        BMP
    }

    [Flags]
    internal enum BitmapExportFlags {
        None = 0,
        CubemapsToSphere = 1 << 0,
        ReconstructZ = 1 << 1,
        FlipGreen = 1 << 2,
    }

    internal sealed class BitmapTag : BaseTag<BitmapFormat> {
        internal Bitmap BitmapData { get; private set; }
        internal bool IsNormalMap => this.GetImporterType() == "normalMap";
        internal bool IsCubeMap => this.GetImporterType() == "cubemap";
        protected override string TagExtension => "bitmap";

        private readonly BitmapExportFlags bitmapFlags;

        internal BitmapTag(TagPath bitmapTagPath, BitmapExportFlags exportOptions = BitmapExportFlags.ReconstructZ | BitmapExportFlags.CubemapsToSphere) : base(bitmapTagPath) {
            this.ValidateTag();

            this.bitmapFlags = exportOptions;

            try {
                Logger.Debug($"{this.TagName}: Building {nameof(GameBitmap)} ...");
                using var gBitmap = new GameBitmap(this.sourceTag, sequenceIndex: 0, spriteFrameIndex: 0);
                this.BitmapData = gBitmap.GetBitmap();
                Logger.Debug($"{this.TagName}: Reading raw image data from tag ...");
            } catch (Exception e) {
                throw new Exception($"Failed to build GameBitmap for `{this.sourceTag.Path.RelativePathWithExtension}`; {e.Message}");
            }
        }

        internal override bool TryExportToDisk(string outputDirectory, BitmapFormat fileExtension, out List<string> finalFileLocations) {
            Logger.Debug($"{this.TagName}: Disk export requested.");

            finalFileLocations = new();

            string extension = fileExtension.ToString().ToLowerInvariant();
            string finalFileLocation = this.BuildOutputPath(outputDirectory, extension);
            if (!string.IsNullOrEmpty(finalFileLocation))
                // If someone's putting this on the root of their drive GetDirectoryName comes back null
                Directory.CreateDirectory(Path.GetDirectoryName(finalFileLocation) ?? outputDirectory);

            var imgFmt = fileExtension switch {
                BitmapFormat.PNG => ImageFormat.Png,
                BitmapFormat.JPG => ImageFormat.Jpeg,
                BitmapFormat.TIFF => ImageFormat.Tiff,
                BitmapFormat.BMP => ImageFormat.Bmp,
                _ => ImageFormat.Png
            };

            if (this.IsCubeMap && this.bitmapFlags.HasFlag(BitmapExportFlags.CubemapsToSphere)) {
                Logger.Debug($"{this.TagName}: Transforming cubemap to equirectangular format ...");
                this.BitmapData = CubemapTools.ToEquirectangular(this.BitmapData);
            }

            if (this.IsNormalMap) {
                bool willReconstructZ = this.bitmapFlags.HasFlag(BitmapExportFlags.ReconstructZ);
                bool willFlipGreen = this.bitmapFlags.HasFlag(BitmapExportFlags.FlipGreen);

                Logger.Debug(
                    $"{this.TagName}: Processing normal map; {(willReconstructZ ? "will" : "will not")} reconstruct Z, " +
                    $"{(willFlipGreen ? "will" : "will not")} flip green channel."
                );

                this.BitmapData = NormalBumpTools.ProcessNormalMap(
                    this.BitmapData,
                    this.bitmapFlags.HasFlag(BitmapExportFlags.ReconstructZ),
                    this.bitmapFlags.HasFlag(BitmapExportFlags.FlipGreen)
                );
            }

            finalFileLocations.Add(finalFileLocation);
            this.BitmapData.Save(finalFileLocation, imgFmt);
            return true;
        }

        private string GetImporterType() {
            var usage = this.sourceTag.SelectFieldType<TagFieldEnum>("Usage");
            int[] normUsages = [2, 3, 18, 19, 20, 21];
            int[] cubeMapUsages = [7];

            if (normUsages.Contains(usage.Value)) return "normalMap";
            if (cubeMapUsages.Contains(usage.Value)) return "cubemap";

            return "default";
        }
    }
}