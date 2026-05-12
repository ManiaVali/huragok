
using System.CommandLine;

namespace Huragok.Application.Commands;

internal class Arguments {
    /// <summary>
    /// The path to a tag file on the disk.
    /// </summary>
    internal static readonly Option<string> TagFile =
        new("--tag", "Path to the tag file.") { Arity = ArgumentArity.ExactlyOne };

    /// <summary>
    /// The paths to a set of tag files on the disk.
    /// </summary>
    internal static readonly Option<string[]> TagFiles =
        new("--tags", "Path to one or more tag files.") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = true };

    /// <summary>
    /// The path to a directory containing tags.
    /// </summary>
    internal static readonly Option<string> TagDirectory =
        new(["--folder", "--dir", "--directory"], "Path to a directory of which all tag files will be targeted.") { Arity = ArgumentArity.ExactlyOne };

    /// <summary>
    /// Whether we should recurse into subdirectories when using `--directory`
    /// </summary>
    internal static readonly Option<bool> TagDirectoryRecurse =
        new(["--recurse"], $"When used with `--folder`, {Application.Constants.PROGRAM_NAME} will also convert tags in subdirectories.");

    /// <summary>
    /// Path to a text file, listing tags line by line to be targeted.
    /// </summary>
    internal static readonly Option<string> TagListFile =
        new("--from-file", "Path to a text file, listing tags line by line to be targeted.");

    /// <summary>
    /// <para>The path to the directory in which tags are exported.</para>
    /// <para>WARNING: Tags are not directly exported here; the original tags-relative path is recreated under this directory.</para>
    /// </summary>
    internal static readonly Option<string> OutDir =
        new(["--out-directory", "--out-dir", "-o"], "Path to the directory to output to, not including file name or extension.") { IsRequired = true };

    /// <summary>
    /// <para>Optional path to a valid configuration file.</para>
    /// <para>Looks in the same folder as the executable by default.</para>
    /// </summary>
    internal static readonly Option<string> ConfigFile =
        new("--config", "Alternate path to a compatible configuration file.");

    /// <summary>
    /// <para>Serialization language to be used. One of `json` or `yaml`.</para>
    /// <para>JSON is default.</para>
    /// </summary>
    internal static readonly Option<string> SerializerFormat =
        new(["--serialization-format", "-s"], "Serialization language to be used. One of `json` (default) or `yaml`.");

    /// <summary>
    /// <para>Used to specify the coordinate system models will be represented in. One of `blam`, `jms` or `metric`.</para>
    /// <para>Metric is default.</para>
    /// </summary>
    internal static readonly Option<string> CoordinateSystem =
        new(["--coordinate-system", "--coords"], "Used to specify the coordinate system 3D data will be represented in. One of `blam`, `jms` or `metric` (default).");

    /// <summary>
    /// Passing this flag causes messages below the severity specified to not be displayed.
    /// </summary>
    internal static readonly Option<string> LogLevel =
        new(["--log-level"], "Passing this flag causes messages below the severity specified to not be displayed. One of `debug`, `info` (default), `warning`, or `error`.");
}