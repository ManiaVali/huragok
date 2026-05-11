
namespace Huragok.Utilities {
    internal class FileTools {
        internal static bool IsSubPathOf(string basePath, string fullPath) {
            basePath = Path.GetFullPath(basePath);
            fullPath = Path.GetFullPath(fullPath);

            string relative = Path.GetRelativePath(basePath, fullPath);

            return !relative.StartsWith("..") && !Path.IsPathRooted(relative);
        }

        internal static string MakeValidFileName(string name) {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] result = new char[name.Length];

            for (int i = 0; i < name.Length; i++) {
                result[i] = Array.IndexOf(invalidChars, name[i]) >= 0 ? '_' : name[i];
            }

            return new string(result);
        }
    }
}