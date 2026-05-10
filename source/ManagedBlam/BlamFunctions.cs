#if USING_BLAM_H4 || USING_BLAM_H2AMP
global using Corinth;
global using Corinth.Tags;
#else
global using Bungie;
global using Bungie.Tags;
#endif
using Huragok.Exceptions.ManagedBlam;
using Huragok.Configuration;

namespace Huragok.ManagedBlam {

    internal static class BlamFunctions {
        private static string TagsFolderPath => Path.Combine(ConfigurationReader.Configuration.ProjectPath, "tags");
        private static bool projectInitialized = false;

        #region Init and Cleanup
        internal static void InitializeBlam() {
            string editingKitPath = ConfigurationReader.Configuration.ProjectPath;

            // Ensure the project is at least somewhat valid. Does the project folder, and tags directory exist and contain data?
            if (!Directory.Exists(editingKitPath)) throw new InvalidBlamProjectException($"{editingKitPath} does not appear to exist.");
            if (!Directory.Exists(TagsFolderPath)) throw new InvalidBlamProjectException($"Tags folder in {editingKitPath} does not appear to exist.\nDid you forget to extract them?");
            if (!Directory.EnumerateFileSystemEntries(TagsFolderPath).Any()) throw new InvalidBlamProjectException($"Tags folder in {editingKitPath} is empty!");
            if (projectInitialized) {
                Logger.Warning("Ignoring request to initialize Blam; already running.");
                return;
            }

            static void crashHandler(ManagedBlamCrashInfo info) { }

            var mbParams = new ManagedBlamStartupParameters { InitializationLevel = InitializationType.TagsOnly };
            ManagedBlamSystem.Start(editingKitPath, crashHandler, mbParams);

            projectInitialized = true;
        }

        internal static void Teardown() {
            if (!projectInitialized) {
                Logger.Error("Cannot shut down ManagedBlam when it's not running!");
                return;
            }

            ManagedBlamSystem.Stop();
            projectInitialized = false;
        }
        #endregion

        internal static bool ValidateTag(TagFile tag, string tagGroupExtension) => ValidateTag(tagPath: tag.Path, tagGroupExtension);

        internal static bool ValidateTag(TagPath tagPath, string tagGroupExtension) {
            string editingKitPath = ConfigurationReader.Configuration.ProjectPath;

            if (!File.Exists(Path.GetFullPath(tagPath.RelativePathWithExtension, Path.Combine(editingKitPath, "tags")))) {
                string failReason = $"Tag `{tagPath.ShortNameWithExtension}` does not seem to exist.";
                throw new FileNotFoundException(failReason);
            }

            if (tagPath.GroupType.Extension != tagGroupExtension) {
                string failReason = $"Tag group is of unexpected type; {tagPath.GroupType.Extension.ToString().ToLower()} is not {tagGroupExtension}!";
                throw new InvalidDataException(failReason);
            }

            return true;
        }

        #region File Helpers
        internal static string GetValidTagPath(string pathToTag) {
            pathToTag = Path.GetFullPath(pathToTag);

            string relPath = Path.GetRelativePath(TagsFolderPath, pathToTag);
            relPath = Path.Combine(
                Path.GetDirectoryName(relPath) ?? "",
                Path.GetFileNameWithoutExtension(relPath)
            );

            return relPath;
        }
        #endregion
    }
}