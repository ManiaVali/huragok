
using System.CommandLine;
using Huragok.Data.Processing.Images;
using Huragok.Data.Tags;

namespace Huragok.Application.Commands.Export;

internal class BitmapExportOptions {
    internal Option<string> ImageFormat { get; }
    internal Option<string> CubeFormat { get; }
    internal Option<bool> ReconstructZ { get; }
    internal Option<bool> FlipGreen { get; }

    private readonly List<Option> allOptions = new();
    internal IReadOnlyList<Option> All => this.allOptions;

    internal BitmapExportOptions() {
        this.ImageFormat = BitmapArguments.ImageFormatOption;
        this.CubeFormat = BitmapArguments.CubmapRepresentationOption;
        this.ReconstructZ = BitmapArguments.NormalRecomputeZOption;
        this.FlipGreen = BitmapArguments.NormalFlipGreenOption;

        this.allOptions.Add(this.ImageFormat);
        this.allOptions.Add(this.CubeFormat);
        this.allOptions.Add(this.ReconstructZ);
        this.allOptions.Add(this.FlipGreen);
    }
}

internal static class BitmapCommandExtensions {
    internal static Command AddBitmapExport(this Command cmd, BitmapExportOptions opts) {
        foreach (var opt in opts.All)
            cmd.Add(opt);

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
        var imgFmt = BitmapTag.StringToExtension(result.GetValue(opts.ImageFormat) ?? "png") ?? BitmapFormat.PNG;
        var cubeFmt = CubemapStringToFormat(result.GetValue(opts.CubeFormat) ?? "raw");

        return new BitmapExportSettings {
            ImageFormat = imgFmt,
            CubeFormat = cubeFmt,
            NrmReconstructZ = result.GetValue(opts.ReconstructZ),
            NrmFlipGreen = result.GetValue(opts.FlipGreen)
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