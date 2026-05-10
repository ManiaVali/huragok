
using System.CommandLine;
using System.CommandLine.Parsing;
using Huragok.Data.IntermediateFormats.Coordinates;
using Huragok.Data.Tags;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok.Commands.RenderModel {
    internal static class ArgsAndOpts {
        /// <summary>
        /// Format which the render model should be written to. One of `glb`, `fbx` and `obj`.
        /// </summary>
        /// <returns>An <see cref="Option"/> containing a model format in the form of a string.</returns>
        internal static Option<string> ModelFormatOption() =>
            new(["-f", "--model-format"], "Format which the RenderModel should be written to. One of `glb`, `fbx` and `obj`.");
    }

    internal class RenderModelExportOptions {
        internal Option<string> ModelFormat { get; }
        internal Option<string> CoordinateSystem { get; }
        private readonly List<Option> allOptions = new();
        internal IReadOnlyList<Option> All => this.allOptions;

        internal RenderModelExportOptions() {
            this.ModelFormat = ArgsAndOpts.ModelFormatOption();
            this.CoordinateSystem = CommonArgsAndOpts.CoordinateSystem;

            this.allOptions.Add(this.ModelFormat);
            this.allOptions.Add(this.CoordinateSystem);
        }
    }

    internal static class CommandExtensions {
        internal static Command AddRenderModelExport(this Command cmd, RenderModelExportOptions opts) {
            foreach (var opt in opts.All)
                cmd.AddOption(opt);

            return cmd;
        }
    }

    internal class RenderModelExportSettings {
        internal RenderModelFormat ModelFormat { get; init; } = RenderModelFormat.FBX;
        internal IF_CoordinateUnit CoordinateSystem { get; init; } = IF_CoordinateUnit.Metric;
    }

    internal static class RenderModelExportResolver {
        internal static RenderModelExportSettings Resolve(this ParseResult result, RenderModelExportOptions opts) {
            var coordSystem = CoordStringToUnit(result.GetValueForOption(opts.CoordinateSystem) ?? "metric");
            var modelFmt = RenderModelTag.StringToExtension(result.GetValueForOption(opts.ModelFormat) ?? "fbx") ?? RenderModelFormat.FBX;

            return new RenderModelExportSettings {
                ModelFormat = modelFmt,
                CoordinateSystem = coordSystem
            };
        }

        private static IF_CoordinateUnit CoordStringToUnit(string unit) {
            return unit.ToLowerInvariant() switch {
                "blam" => IF_CoordinateUnit.Blam,
                "jms" => IF_CoordinateUnit.JMS,
                "metric" => IF_CoordinateUnit.Metric,
                _ => throw new ArgumentException($"Invalid model format type `{unit.ToLowerInvariant()}`.")
            };
        }
    }
}