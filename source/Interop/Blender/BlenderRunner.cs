
using System.Diagnostics;

namespace Huragok.Utilities.Blender {
    internal class Runner {
        internal static void GLB2FBX(string glbFile, string outFbxLocation) {

            if (!Path.Exists(Path.GetFullPath(glbFile))) throw new FileNotFoundException($"Failed to convert to FBX; glb file expected at `{glbFile}` not found!");

            string blenderPath = Path.GetFullPath(Locator.FindBlender());
            string scriptPath = Path.Combine(AppContext.BaseDirectory, "utils", "blender", "glb2fbx.py");

            var startInfo = new ProcessStartInfo {
                FileName = blenderPath,
                Arguments =
                    $"--background --factory-startup --disable-autoexec --python \"{scriptPath}\" -- " +
                    $"\"{glbFile}\" " +
                    $"\"{outFbxLocation}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var blender = new Process { StartInfo = startInfo };
            blender.Start();

            var stdOutTask = blender.StandardOutput.ReadToEndAsync();
            var stdErrTask = blender.StandardError.ReadToEndAsync();

            blender.WaitForExit();

            string stdOut = stdOutTask.Result;
            string stdErr = stdErrTask.Result;

            blender.WaitForExit();
            if (blender.ExitCode != 0) {
                if (File.Exists(outFbxLocation)) File.Delete(outFbxLocation);
                throw new Exception($"Blender failed to convert to FBX!");
            }
        }
    }
}
