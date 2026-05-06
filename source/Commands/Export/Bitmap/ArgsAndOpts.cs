
using System.CommandLine;
using System.CommandLine.Parsing;
using Huragok.Data.Tags;
using Huragok.Utilities.Imaging;

namespace Huragok.Commands.Bitmaps {
    internal static class ArgsAndOpts {
        /// <summary>
        /// Format which the bitmap should be written to. One of `png`, `bmp`, `tiff` and `jpg`.
        /// </summary>
        /// <returns>An <see cref="Option"/> containing an image format in the form of a string.</returns>
        internal static readonly Option<string> ImageFormatOption =
            new(["-f", "--image-format"], "Format which the bitmap should be written to. One of `png`, `bmp`, `tiff` and `jpg`.");

        /// <summary>
        /// Used to specify a format for extracted cubemaps. One of `raw` (default) or `equirectangular`.
        /// </summary>
        /// <returns>An <see cref="Option"/> containing a cubemap format in the form of a string.</returns>
        internal static readonly Option<string> CubmapRepresentationOption =
            new(["--cubemap-layout"], "Used to specify a format for extracted cubemaps. One of `raw` (default) or `equirectangular`. No effect on bitmaps that are not cubemaps.");

        /// <summary>
        /// Do not recompute the Z channel of extracted normal maps.
        /// </summary>
        /// <returns>An <see cref="Option"/> containing a bool; if true, Z rebuilding is disabled.</returns>
        internal static readonly Option<bool> NormalRecomputeZOption =
            new(["--normal-fix"], "Recompute the Z channel of extracted normal maps. No effect on bitmaps that are not normal maps.");

        /// <summary>
        /// Flip the green channel of extracted normal maps.
        /// </summary>
        /// <returns>An <see cref="Option"/> containing a bool; if true, normal maps will have their green channel inverted.</returns>
        internal static readonly Option<bool> NormalFlipGreenOption =
            new(["--normal-flip-green"], "Flip the green channel of extracted normal maps (converts from DirectX to OpenGL normals). No effect on bitmaps that are not normal maps.");
    }

    internal class BitmapExportOptions {
        internal Option<string> ImageFormat { get; }
        internal Option<string> CubeFormat { get; }
        internal Option<bool> ReconstructZ { get; }
        internal Option<bool> FlipGreen { get; }

        private readonly List<Option> allOptions = new();
        internal IReadOnlyList<Option> All => this.allOptions;

        internal BitmapExportOptions() {
            this.ImageFormat = ArgsAndOpts.ImageFormatOption;
            this.CubeFormat = ArgsAndOpts.CubmapRepresentationOption;
            this.ReconstructZ = ArgsAndOpts.NormalRecomputeZOption;
            this.FlipGreen = ArgsAndOpts.NormalFlipGreenOption;

            this.allOptions.Add(this.ImageFormat);
            this.allOptions.Add(this.CubeFormat);
            this.allOptions.Add(this.ReconstructZ);
            this.allOptions.Add(this.FlipGreen);
        }
    }

    internal static class CommandExtensions {
        internal static Command AddBitmapExport(this Command cmd, BitmapExportOptions opts) {
            foreach (var opt in opts.All)
                cmd.AddOption(opt);

            return cmd;
        }
    }

    internal class BitmapExportSettings {
        internal BitmapFormat ImageFormat { get; init; } = BitmapFormat.PNG;
        internal CubemapFormat CubeFormat { get; init; } = CubemapFormat.Raw;
        internal bool NrmReconstructZ { get; init; } = true;
        internal bool NrmFlipGreen { get; init; }
    }

    internal static class BitmapExportResolver {
        internal static BitmapExportSettings Resolve(this ParseResult result, BitmapExportOptions opts) {
            var imgFmt = BitmapTag.StringToExtension(result.GetValueForOption(opts.ImageFormat) ?? "png") ?? BitmapFormat.PNG;
            var cubeFmt = CubemapStringToFormat(result.GetValueForOption(opts.CubeFormat) ?? "raw");

            return new BitmapExportSettings {
                ImageFormat = imgFmt,
                CubeFormat = cubeFmt,
                NrmReconstructZ = result.GetValueForOption(opts.ReconstructZ),
                NrmFlipGreen = result.GetValueForOption(opts.FlipGreen)
            };
        }

        private static CubemapFormat CubemapStringToFormat(string format) {
            return format.ToLowerInvariant() switch {
                "raw" => CubemapFormat.Raw,
                "equirectangular" => CubemapFormat.Equirectangular,
                _ => throw new ArgumentException($"Invalid cubemap format type `{format.ToLowerInvariant()}`.")
            };
        }
    }
}