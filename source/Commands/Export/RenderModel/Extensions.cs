
using Fmod5Sharp.FmodTypes;
using SharpGLTF.Schema2;

namespace Huragok.Commands.RenderModel {
    internal static class GLTFExtensions {
        internal static void SaveFBX(this ModelRoot model, string fbxLocation) {
            string tempPath = Path.GetTempFileName();
            model.SaveGLB(tempPath);

            Utilities.Blender.Runner.GLB2FBX(tempPath, fbxLocation);
            File.Delete(tempPath);
        }
    }
}