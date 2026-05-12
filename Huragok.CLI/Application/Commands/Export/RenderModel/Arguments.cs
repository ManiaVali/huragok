
using System.CommandLine;
using Huragok.Data.RuntimeFormats;
using Huragok.Data.Tags;

namespace Huragok.Application.Commands.Export;

internal static class RenderModelArguments {
    /// <summary>
    /// Format which the render model should be written to. One of `glb`, `fbx` and `obj`.
    /// </summary>
    /// <returns>An <see cref="Option"/> containing a model format in the form of a string.</returns>
    internal static Option<string> ModelFormatOption() =>
        new(name: "--model-format", aliases: ["-f"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "glb, fbx, or obj",
            DefaultValueFactory = _ => RenderModelFormat.GLB.ToString().ToLower(),
            Description = "Format which the RenderModel should be written to."
        };
}

internal class RenderModelExportOptions {
    internal Option<string> ModelFormat { get; }
    internal Option<string> CoordinateSystem { get; }
    private readonly List<Option> allOptions = new();
    internal IReadOnlyList<Option> All => this.allOptions;

    internal RenderModelExportOptions() {
        this.ModelFormat = RenderModelArguments.ModelFormatOption();
        this.CoordinateSystem = Commands.Arguments.CoordinateSystem;

        this.allOptions.Add(this.ModelFormat);
        this.allOptions.Add(this.CoordinateSystem);
    }
}

internal static class CommandExtensions {
    internal static Command AddRenderModelExport(this Command cmd, RenderModelExportOptions opts) {
        foreach (var opt in opts.All)
            cmd.Add(opt);

        return cmd;
    }
}

internal class RenderModelExportSettings {
    internal RenderModelFormat ModelFormat { get; init; } = RenderModelFormat.FBX;
    internal CoordinateUnit CoordinateSystem { get; init; } = CoordinateUnit.Metric;
}

internal static class RenderModelExportResolver {
    internal static RenderModelExportSettings Resolve(this ParseResult result, RenderModelExportOptions opts) {
        var coordSystem = CoordStringToUnit(result.GetValue(opts.CoordinateSystem) ?? "metric");
        var modelFmt = RenderModelTag.StringToExtension(result.GetValue(opts.ModelFormat) ?? "fbx") ?? RenderModelFormat.FBX;

        return new RenderModelExportSettings {
            ModelFormat = modelFmt,
            CoordinateSystem = coordSystem
        };
    }

    private static CoordinateUnit CoordStringToUnit(string unit) {
        return unit.ToLowerInvariant() switch {
            "blam" => CoordinateUnit.Blam,
            "jms" => CoordinateUnit.JMS,
            "metric" => CoordinateUnit.Metric,
            _ => throw new ArgumentException($"Invalid model format type `{unit.ToLowerInvariant()}`.")
        };
    }
}