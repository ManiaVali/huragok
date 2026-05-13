
using System.CommandLine;
using Huragok.Data.Processing.Images;
using Huragok.Data.Tags;

namespace Huragok.Application.Commands.Export;

internal static class BitmapArguments {
    /// <summary>
    /// Format which the bitmap should be written to. One of `png`, `bmp`, `tiff` and `jpg`.
    /// </summary>
    /// <returns>An <see cref="Option"/> containing an image format in the form of a string.</returns>
    internal static readonly Option<string> ImageFormatOption =
        new(name: "--image-format", aliases: ["-f"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "png, bmp, tiff, jpg",
            DefaultValueFactory = _ => BitmapFormat.PNG.ToString().ToLower(),
            Description = "Format which the bitmap should be written to."
        };

    /// <summary>
    /// Used to specify a format for extracted cubemaps. One of `raw` (default) or `equirectangular`.
    /// </summary>
    /// <returns>An <see cref="Option"/> containing a cubemap format in the form of a string.</returns>
    internal static readonly Option<string> CubmapRepresentationOption =
        new(name: "--cubemap-layout") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "raw or equirectangular",
            DefaultValueFactory = _ => CubemapFormat.Raw.ToString().ToLower(),
            Description = "Used to specify a format for extracted cubemaps."
        };

    /// <summary>
    /// Do not recompute the Z channel of extracted normal maps.
    /// </summary>
    /// <returns>An <see cref="Option"/> containing a bool; if true, Z rebuilding is disabled.</returns>
    internal static readonly Option<bool> NormalRecomputeZOption =
        new(name: "--normal-fix") {
            Arity = ArgumentArity.ExactlyOne,
            Description = "Recompute the Z channel of extracted normal maps. No effect on bitmaps that are not normal maps."
        };

    /// <summary>
    /// Flip the green channel of extracted normal maps.
    /// </summary>
    /// <returns>An <see cref="Option"/> containing a bool; if true, normal maps will have their green channel inverted.</returns>
    internal static readonly Option<bool> NormalFlipGreenOption =
        new(name: "--normal-flip-green") {
            Arity = ArgumentArity.ExactlyOne,
            Description = "Flip the green channel of extracted normal maps (converts from DirectX to OpenGL normals). No effect on bitmaps that are not normal maps."
        };
}