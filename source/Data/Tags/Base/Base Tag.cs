using Huragok.Configuration;
using Huragok.Exceptions.ManagedBlam;
using Huragok.ManagedBlam;
using Huragok.Utilities;

namespace Huragok.Data.Tags {
    internal enum BaseTagExtension {
        None
    }

    /// <summary>
    /// Base representation of a Tag used during the export process.
    /// </summary>
    /// <typeparam name="TFileExt">File extension enum.</typeparam>
    internal abstract class BaseTag<TFileExt> : IDisposable where TFileExt : struct, Enum {
        internal readonly TagFile sourceTag;
        protected abstract string TagExtension { get; }
        protected string TagRelPath => this.sourceTag.Path.RelativePath;
        protected string TagName => Path.GetFileName(this.sourceTag.Path.RelativePathWithExtension);
        protected string TagNameNoExtension => Path.ChangeExtension(this.TagName, null);

        private bool _disposed;

        internal BaseTag(TagPath tagPath) {
            string fullPath = Path.GetFullPath(Path.Combine("tags", tagPath.RelativePathWithExtension));

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Unable to open `{fullPath}`; no such file or directory.");

            if (!FileTools.IsSubPathOf(ConfigurationReader.Configuration.ProjectPath, tagPath.RelativePathWithExtension))
                throw new MismatchedBlamProjectException($"Refusing to open `{fullPath}` as it is beyond the editing kit path `{ConfigurationReader.Configuration.ProjectPath}`!");

            var tagFile = new TagFile(tagPath); // No `using` -- manually disposed when we are
            this.sourceTag = tagFile;
        }

        // TODO: Find a way to validate the tag data other than just its ext
        internal bool ValidateTag() => BlamFunctions.ValidateTag(this.sourceTag, this.TagExtension);

        internal virtual bool TryExportToDisk(string outputDirectory, TFileExt fileExtension, out List<string> finalFileLocations) => throw new NotSupportedException($"Export of {this.GetType().Name} not supported!");

        internal virtual string BuildOutputPath(string outputDirectory, string extension) =>
            Path.GetFullPath(Path.Combine(outputDirectory, Path.GetDirectoryName(this.TagRelPath) ?? "", $"{this.sourceTag.Path.ShortName}.{extension}"), MainProgram.originalWorkingDirectory);

        internal static TFileExt? StringToExtension(string extensionAsString) {
            if (Enum.TryParse<TFileExt>(extensionAsString, ignoreCase: true, out var result)) {
                return result;
            }

            return null;
        }

        internal static T SetFlag<T>(T value, T flag, bool enabled) where T : struct, Enum {
            long intValue = Convert.ToInt64(value);
            long intFlag = Convert.ToInt64(flag);

            if (enabled)
                intValue |= intFlag;   // set flag
            else
                intValue &= ~intFlag;  // clear flag

            return (T)Enum.ToObject(typeof(T), intValue);
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (this._disposed) return;
            if (disposing) this.sourceTag?.Dispose();

            this._disposed = true;
        }

        ~BaseTag() {
            this.Dispose(false);
        }
    }
}