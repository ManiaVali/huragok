using SharpGLTF.Schema2;

namespace Huragok.Commands.RenderModel {
    internal static class GLTFExtensions {
        /// <summary>
        /// <para>Extension method for GLTFSharp, allowing exporting of GLTF <see cref="ModelRoot"/>s to FBX.</para>
        /// <para>Relies on blender and <see cref="Utilities.Blender.Runner.GLB2FBX(string, string)"/> </para>
        /// </summary>
        /// <param name="model">A GLTF ModelRoot</param>
        /// <param name="fbxLocation">The final file path to the desired FBX.</param>
        internal static void SaveFBX(this ModelRoot model, string fbxLocation) {
            string tempPath = Path.GetTempFileName();
            model.SaveGLB(tempPath);

            Utilities.Blender.Runner.GLB2FBX(tempPath, fbxLocation);
            File.Delete(tempPath);
        }
    }
}