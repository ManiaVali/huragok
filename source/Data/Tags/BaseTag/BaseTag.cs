using Huragok.Exceptions.ManagedBlam;
using Huragok.ManagedBlam;
using Huragok.Configuration;
using Huragok.Utilities;

namespace Huragok.Data.Tags {
    public enum BaseTagExtension {
        None
    }

    /// <summary>
    /// Base representation of a Tag used during the export process.
    /// </summary>
    /// <typeparam name="TFileExt">File extension enum.</typeparam>
    public abstract class BaseTag<TFileExt> : IDisposable where TFileExt : struct, Enum {
        public readonly TagFile sourceTag;
        protected abstract string TagExtension { get; }
        protected readonly string tagRelPath;

        private bool _disposed;

        public BaseTag(TagPath tagPath) {
            string fullPath = Path.GetFullPath(Path.Combine("tags", tagPath.RelativePathWithExtension));

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Unable to open `{fullPath}`; no such file or directory.");

            if (!FileTools.IsSubPathOf(ConfigurationReader.Configuration.ProjectPath, tagPath.RelativePathWithExtension))
                throw new MismatchedBlamProjectException($"Refusing to open `{fullPath}` as it is beyond the editing kit path `{ConfigurationReader.Configuration.ProjectPath}`!");

            var tagFile = new TagFile(tagPath); // No `using` -- manually disposed when we are
            this.sourceTag = tagFile;

            this.tagRelPath = this.sourceTag.Path.RelativePath;
        }

        // TODO: Find a way to validate the tag data other than just its ext
        public bool ValidateTag() => BlamFunctions.ValidateTag(this.sourceTag, this.TagExtension);

        public virtual bool TryExportToDisk(string outputDirectory, TFileExt fileExtension, out List<string> finalFileLocations) => throw new NotSupportedException($"Export of {this.GetType().Name} not supported!");

        public virtual string BuildOutputPath(string outputDirectory, string extension) =>
            Path.GetFullPath(Path.Combine(outputDirectory, Path.GetDirectoryName(this.tagRelPath) ?? "", $"{this.sourceTag.Path.ShortName}.{extension}"), MainProgram.originalWorkingDirectory);

        public static TFileExt? StringToExtension(string extensionAsString) {
            if (Enum.TryParse<TFileExt>(extensionAsString, ignoreCase: true, out var result)) {
                return result;
            }

            return null;
        }

        public static T SetFlag<T>(T value, T flag, bool enabled) where T : struct, Enum {
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