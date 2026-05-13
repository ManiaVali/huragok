using System.CommandLine;

namespace Huragok.Application.Commands;

/// <summary>
/// Class containing all possible tag input options; used alongside <see cref="CommandExtensions.AddTagInput"/>> to dynamically add tag input types to a command handler.
/// </summary>
internal class TagInputOptions {
    internal Option<string>? TagFile { get; }
    internal Option<string[]>? TagFiles { get; }
    internal Option<string>? TagDirectory { get; }
    internal Option<bool>? DirectoryRecurse { get; }
    internal Option<string>? TagListFile { get; }

    private readonly List<Option> _allOptions = new();

    internal IReadOnlyList<Option> All => this._allOptions;

    internal TagInputOptions(
        bool allowSingle = true,
        bool allowMultiple = true,
        bool allowDirectory = true,
        bool allowListFile = true,
        bool allowRecurse = true) {
        if (allowSingle) {
            this.TagFile = Arguments.TagFile;
            this._allOptions.Add(this.TagFile);
        }

        if (allowMultiple) {
            this.TagFiles = Arguments.TagFiles;
            this._allOptions.Add(this.TagFiles);
        }

        if (allowDirectory) {
            this.TagDirectory = Arguments.TagDirectory;
            this._allOptions.Add(this.TagDirectory);

            if (allowRecurse) {
                this.DirectoryRecurse = Arguments.TagDirectoryRecurse;
                this._allOptions.Add(this.DirectoryRecurse);
            }
        }

        if (allowListFile) {
            this.TagListFile = Arguments.TagListFile;
            this._allOptions.Add(this.TagListFile);
        }
    }
}

internal static class CommandExtensions {
    internal static Command AddTagInput(this Command cmd, TagInputOptions opts) {
        foreach (var opt in opts.All)
            cmd.Add(opt);

        cmd.Validators.Add(result => {
            int count = 0;

            if (opts.TagFile != null && result.GetResult(opts.TagFile) != null) count++;
            if (opts.TagFiles != null && result.GetResult(opts.TagFiles) != null) count++;
            if (opts.TagDirectory != null && result.GetResult(opts.TagDirectory) != null) count++;
            if (opts.TagListFile != null && result.GetResult(opts.TagListFile) != null) count++;

            if (count == 0) {
                result.AddError("You must specify one tag input source.");
                return;
            }

            if (count > 1) {
                result.AddError("Tag input options are mutually exclusive.");
                return;
            }

            if (opts.DirectoryRecurse != null &&
                result.GetResult(opts.DirectoryRecurse) != null &&
                result.GetResult(opts.TagDirectory!) == null) {
                result.AddError("--recurse requires --directory.");
            }
        });

        return cmd;
    }

    internal static TagInputResult Resolve(this ParseResult result, TagInputOptions opts) {
        if (opts.TagFile != null && !string.IsNullOrWhiteSpace(result.GetValue(opts.TagFile))) {
            string single = result.GetRequiredValue(opts.TagFile);

            return new() { Paths = new[] { single } };
        }

        if (opts.TagFiles != null && result.GetValue(opts.TagFiles)?.Length > 0) {
            string[] many = result.GetRequiredValue(opts.TagFiles);

            return new() { Paths = many };
        }

        if (opts.TagListFile != null && !string.IsNullOrWhiteSpace(result.GetValue(opts.TagListFile))) {
            string file = result.GetRequiredValue(opts.TagListFile);

            if (!File.Exists(Path.GetFullPath(file)))
                throw new FileNotFoundException($"Unable to open `{file}`; no such file or directory.");

            return new() { Paths = File.ReadAllLines(file) };
        }

        if (opts.TagDirectory != null && !string.IsNullOrWhiteSpace(result.GetValue(opts.TagDirectory))) {
            string dir = result.GetRequiredValue(opts.TagDirectory);

            if (!Directory.Exists(Path.GetFullPath(dir)))
                throw new FileNotFoundException($"Unable to open `{Path.GetFullPath(dir)}`; no such file or directory.");

            bool recurse = opts.DirectoryRecurse != null && result.GetValue(opts.DirectoryRecurse);

            return new() {
                Paths = Directory.GetFiles(dir, "*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            };
        }

        throw new InvalidOperationException();
    }
}

internal class TagInputResult {
    internal IEnumerable<string> Paths { get; init; } = Enumerable.Empty<string>();
}