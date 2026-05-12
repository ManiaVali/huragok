
using System.CommandLine;
using Huragok.Application.Logging;
using Huragok.Data.RuntimeFormats;
using Huragok.Data.Serialization;

namespace Huragok.Application.Commands;

internal class Arguments {
    /// <summary>
    /// The path to a tag file on the disk.
    /// </summary>
    internal static readonly Option<string> TagFile =
        new(name: "--tag") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "tag file",
            Description = "Path to a single tag file."
        };

    /// <summary>
    /// The paths to a set of tag files on the disk.
    /// </summary>
    internal static readonly Option<string[]> TagFiles =
        new(name: "--tags") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "tag files",
            AllowMultipleArgumentsPerToken = true,
            Description = "Path to one or more tag files."
        };

    /// <summary>
    /// The path to a directory containing tags.
    /// </summary>
    internal static readonly Option<string> TagDirectory =
        new(name: "--directory", aliases: ["--folder", "--dir"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "directory full of tags",
            Description = "Path to a directory of which all tag files will be targeted."
        };

    /// <summary>
    /// Whether we should recurse into subdirectories when using `--directory`
    /// </summary>
    internal static readonly Option<bool> TagDirectoryRecurse =
        new(name: "--recurse") {
            Arity = ArgumentArity.ExactlyOne,
            Description = $"When used with `--folder`, {Application.Constants.PROGRAM_NAME} will also convert tags in subdirectories."
        };

    /// <summary>
    /// Path to a text file, listing tags line by line to be targeted.
    /// </summary>
    internal static readonly Option<string> TagListFile =
        new(name: "--from-file") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "tag list file",
            Description = "Path to a text file, listing tags line by line to be targeted."
        };

    /// <summary>
    /// <para>The path to the directory in which tags are exported.</para>
    /// <para>WARNING: Tags are not directly exported here; the original tags-relative path is recreated under this directory.</para>
    /// </summary>
    internal static readonly Option<string> OutDir =
        new(name: "--out-directory", aliases: ["--out-dir", "-o"]) {
            Required = true,
            HelpName = "output directory",
            Arity = ArgumentArity.ExactlyOne,
            Description = "Path to the directory to output to, not including file name or extension."
        };

    /// <summary>
    /// <para>Optional path to a valid configuration file.</para>
    /// <para>Looks in the same folder as the executable by default.</para>
    /// </summary>
    internal static readonly Option<string> ConfigFile =
        new(name: "--config") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "configuration file",
            DefaultValueFactory = _ => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "config", "HuragokConfiguration.json")),
            Description = "Alternate path to a compatible configuration file."
        };

    /// <summary>
    /// <para>Serialization language to be used. One of `json` or `yaml`.</para>
    /// <para>JSON is default.</para>
    /// </summary>
    internal static readonly Option<string> SerializerFormat =
        new(name: "--serialization-format", aliases: ["-s"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "json or yaml",
            DefaultValueFactory = _ => SerializationFormat.JSON.ToString().ToLower(),
            Description = "Serialization language to be used."
        };

    /// <summary>
    /// <para>Used to specify the coordinate system models will be represented in. One of `blam`, `jms` or `metric`.</para>
    /// <para>Metric is default.</para>
    /// </summary>
    internal static readonly Option<string> CoordinateSystem =
        new(name: "--coordinate-system", aliases: "--coords") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "blam, jms or metric",
            DefaultValueFactory = _ => CoordinateUnit.Metric.ToString().ToLower(),
            Description = "Used to specify the coordinate system 3D data will be represented in."
        };

    /// <summary>
    /// Passing this flag causes messages below the severity specified to not be displayed.
    /// </summary>
    internal static readonly Option<string> LogLevel =
        new(name: "--log-level") {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "debug, info, warning or error",
            DefaultValueFactory = _ => LoggingLevel.Info.ToString().ToLower(),
            Description = "Passing this flag causes messages below the severity specified to not be displayed."
        };
}