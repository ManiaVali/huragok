namespace Huragok.Interop.Blender;

internal static class Locator {
    internal static string FindBlender() => FindBlenderFromPath() ?? FindBlenderFromInstallDirs() ?? throw new FileNotFoundException($"Cannot find a valid blender executable! Is blender installed?");

    private static string? FindBlenderFromPath() {
        string[] paths = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator);

        foreach (string path in paths) {
            try {
                string fullPath = Path.Combine(path, "blender.exe");
                if (File.Exists(fullPath))
                    return fullPath;
            } catch { }
        }

        return null;
    }

    private static string? FindBlenderFromInstallDirs() {
        string[] possibleDirs = {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Blender Foundation"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Blender Foundation"),
        };

        foreach (string dir in possibleDirs) {
            if (Directory.Exists(dir)) {
                string? exe = Directory.GetFiles(dir, "blender.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (exe != null) return exe;
            }
        }

        return null;
    }
}
