
using System.CommandLine;
using System.CommandLine.Parsing;
using Huragok.Utilities;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok.Commands.Base {
    public class ArgsAndOpts {
        /// <summary>
        /// The path to a tag file on the disk.
        /// </summary>
        public static readonly Option<string> TagFile =
            new("--tag", "Path to the tag file.") { Arity = ArgumentArity.ExactlyOne };

        /// <summary>
        /// The paths to a set of tag files on the disk.
        /// </summary>
        public static readonly Option<string[]> TagFiles =
            new("--tags", "Path to one or more tag files.") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = true };

        /// <summary>
        /// The paths to a set of tag files on the disk.
        /// </summary>
        public static readonly Option<string> TagDirectory =
            new(["--folder", "--dir", "--directory"], "Path to a directory of which all tag files will be targeted.") { Arity = ArgumentArity.ExactlyOne };

        /// <summary>
        /// Whether we should recurse into subdirectories when using `--directory`
        /// </summary>
        public static readonly Option<bool> TagDirectoryRecurse =
            new(["--recurse"], $"When used with `--folder`, {GlobalConstants.PROGRAM_NAME} will also convert tags in subdirectories.");

        /// <summary>
        /// Path to a text file, listing tags line by line to be targeted.
        /// </summary>
        public static readonly Option<string> TagListFile =
            new("--from-file", "Path to a text file, listing tags line by line to be targeted.");

        /// <summary>
        /// <para>The path to the directory in which tags are exported.</para>
        /// <para>WARNING: Tags are not directly exported here; the original tags-relative path is recreated under this directory.</para>
        /// </summary>
        public static readonly Option<string> OutDir =
            new(["--out-directory", "--out-dir", "-o"], "Path to the directory to output to, not including file name or extension.");

        /// <summary>
        /// <para>Optional path to a valid configuration file.</para>
        /// <para>Looks in the same folder as the executable by default.</para>
        /// </summary>
        public static readonly Option<string> ConfigFile =
            new("--config", "Alternate path to a compatible configuration file.");

        /// <summary>
        /// <para>Serialization language to be used. One of `json` or `yaml`.</para>
        /// <para>JSON is default.</para>
        /// </summary>
        public static readonly Option<string> SerializerFormat =
            new(["--serialization-format", "-s"], "Serialization language to be used. One of `json` (default) or `yaml`.");

        /// <summary>
        /// <para>Used to specify the coordinate system models will be represented in. One of `blam`, `jms` or `metric`.</para>
        /// <para>Metric is default.</para>
        /// </summary>
        public static readonly Option<string> CoordinateSystem =
            new(["--coordinate-system", "--coords"], "Used to specify the coordinate system models will be represented in. One of `blam`, `jms` or `metric` (default).");
    }

    internal class TagInputOptions {
        public Option<string>? TagFile { get; }
        public Option<string[]>? TagFiles { get; }
        public Option<string>? TagDirectory { get; }
        public Option<bool>? DirectoryRecurse { get; }
        public Option<string>? TagListFile { get; }

        private readonly List<Option> allOptions = new();

        public IReadOnlyList<Option> All => this.allOptions;

        public TagInputOptions(
            bool allowSingle = true,
            bool allowMultiple = true,
            bool allowDirectory = true,
            bool allowListFile = true,
            bool allowRecurse = true) {
            if (allowSingle) {
                this.TagFile = CommonArgsAndOpts.TagFile;
                this.allOptions.Add(this.TagFile);
            }

            if (allowMultiple) {
                this.TagFiles = CommonArgsAndOpts.TagFiles;
                this.allOptions.Add(this.TagFiles);
            }

            if (allowDirectory) {
                this.TagDirectory = CommonArgsAndOpts.TagDirectory;
                this.allOptions.Add(this.TagDirectory);

                if (allowRecurse) {
                    this.DirectoryRecurse = CommonArgsAndOpts.TagDirectoryRecurse;
                    this.allOptions.Add(this.DirectoryRecurse);
                }
            }

            if (allowListFile) {
                this.TagListFile = CommonArgsAndOpts.TagListFile;
                this.allOptions.Add(this.TagListFile);
            }
        }
    }

    internal static class CommandExtensions {
        public static Command AddTagInput(this Command cmd, TagInputOptions opts) {
            foreach (var opt in opts.All)
                cmd.AddOption(opt);

            cmd.AddValidator(result => {
                int count = 0;

                if (opts.TagFile != null && result.FindResultFor(opts.TagFile) != null) count++;
                if (opts.TagFiles != null && result.FindResultFor(opts.TagFiles) != null) count++;
                if (opts.TagDirectory != null && result.FindResultFor(opts.TagDirectory) != null) count++;
                if (opts.TagListFile != null && result.FindResultFor(opts.TagListFile) != null) count++;

                if (count == 0) {
                    result.ErrorMessage = "You must specify one tag input source.";
                    return;
                }

                if (count > 1) {
                    result.ErrorMessage = "Tag input options are mutually exclusive.";
                    return;
                }

                if (opts.DirectoryRecurse != null &&
                    result.FindResultFor(opts.DirectoryRecurse) != null &&
                    result.FindResultFor(opts.TagDirectory!) == null) {
                    result.ErrorMessage = "--recurse requires --directory.";
                }
            });

            return cmd;
        }

        public static TagInputResult Resolve(this ParseResult result, TagInputOptions opts) {
            if (opts.TagFile != null && !string.IsNullOrWhiteSpace(result.GetValueForOption(opts.TagFile))) {
                string single = result.GetValueForOption(opts.TagFile)
                    ?? throw new ArgumentException($"Error in `{nameof(CommandExtensions)}.{nameof(Resolve)}`; TagFile cannot be null.");

                return new() { Paths = new[] { single } };
            }

            if (opts.TagFiles != null && result.GetValueForOption(opts.TagFiles)?.Length > 0) {
                string[] many = result.GetValueForOption(opts.TagFiles)
                    ?? throw new ArgumentException($"Error in `{nameof(CommandExtensions)}.{nameof(Resolve)}`; TagFiles cannot be null.");

                return new() { Paths = many };
            }

            if (opts.TagListFile != null && !string.IsNullOrWhiteSpace(result.GetValueForOption(opts.TagListFile))) {
                string file = result.GetValueForOption(opts.TagListFile)
                    ?? throw new ArgumentException($"Error in `{nameof(CommandExtensions)}.{nameof(Resolve)}`; TagListFile cannot be null.");

                if (!File.Exists(Path.GetFullPath(file)))
                    throw new FileNotFoundException($"Unable to open `{file}`; no such file or directory.");

                return new() { Paths = File.ReadAllLines(file) };
            }

            if (opts.TagDirectory != null && !string.IsNullOrWhiteSpace(result.GetValueForOption(opts.TagDirectory))) {
                string dir = result.GetValueForOption(opts.TagDirectory)
                    ?? throw new ArgumentException($"Error in `{nameof(CommandExtensions)}.{nameof(Resolve)}`; TagDirectory cannot be null.");

                if (!Directory.Exists(Path.GetFullPath(dir)))
                    throw new FileNotFoundException($"Unable to open `{Path.GetFullPath(dir)}`; no such file or directory.");

                bool recurse = opts.DirectoryRecurse != null && result.GetValueForOption(opts.DirectoryRecurse);

                return new() {
                    Paths = Directory.GetFiles(dir, "*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                };
            }

            throw new InvalidOperationException();
        }
    }

    internal class TagInputResult {
        public IEnumerable<string> Paths { get; init; } = Enumerable.Empty<string>();
    }
}