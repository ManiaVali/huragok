
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
        public Option<string> ModelFormat { get; }
        public Option<string> CoordinateSystem { get; }
        private readonly List<Option> allOptions = new();
        public IReadOnlyList<Option> All => this.allOptions;

        public RenderModelExportOptions() {
            this.ModelFormat = ArgsAndOpts.ModelFormatOption();
            this.CoordinateSystem = CommonArgsAndOpts.CoordinateSystem;

            this.allOptions.Add(this.ModelFormat);
            this.allOptions.Add(this.CoordinateSystem);
        }
    }

    internal static class CommandExtensions {
        public static Command AddRenderModelExport(this Command cmd, RenderModelExportOptions opts) {
            foreach (var opt in opts.All)
                cmd.AddOption(opt);

            return cmd;
        }
    }

    internal class RenderModelExportSettings {
        public RenderModelFormat ModelFormat { get; init; } = RenderModelFormat.FBX;
        public CoordinateUnit CoordinateSystem { get; init; } = CoordinateUnit.Metric;
    }

    internal static class RenderModelExportResolver {
        public static RenderModelExportSettings Resolve(this ParseResult result, RenderModelExportOptions opts) {
            var coordSystem = CoordStringToUnit(result.GetValueForOption(opts.CoordinateSystem) ?? "metric");
            var modelFmt = RenderModelTag.StringToExtension(result.GetValueForOption(opts.ModelFormat) ?? "fbx") ?? RenderModelFormat.FBX;

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
}