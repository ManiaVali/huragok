import bpy
import sys

bpy.ops.preferences.addon_enable(module="io_scene_gltf2")
bpy.ops.preferences.addon_enable(module="io_scene_fbx")

argv = sys.argv[sys.argv.index("--") + 1:]

input_file = argv[0]
output_file = argv[1]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=input_file)

col = bpy.data.collections.get("glTF_not_exported")
if col:
    for obj in list(col.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(col)


bpy.ops.export_scene.fbx(filepath=output_file)