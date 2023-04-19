import bpy
import os
import sys
from math import radians


def parseAndExport():
    sys.argv = sys.argv[sys.argv.index("--") + 1:]  # get all args after "--"
    print(sys.argv)
    filePath = sys.argv[0]
    rotAxis = sys.argv[1]
    rotAmount = float(sys.argv[2])
    exportPath = sys.argv[3]
    exportFormat = sys.argv[4]
    file = os.path.basename(filePath)
    fileName, fileExt = os.path.splitext(file)

    if exportFormat == "":
        exportFormat = fileExt

    newfile = os.path.join(exportPath, fileName + exportFormat)
    print(filePath)
    print(fileExt)
    print(newfile)
    if fileExt == ".fbx":
        bpy.ops.scene.new(type="NEW")
        bpy.ops.import_scene.fbx(filepath=filePath)
    if fileExt == ".obj":
        bpy.ops.scene.new(type="NEW")
        bpy.ops.import_scene.obj(filepath=filePath)
    elif fileExt == ".glb" or fileExt == ".gltf":
        bpy.ops.scene.new(type="NEW")
        bpy.ops.import_scene.gltf(filepath=filePath)
    elif fileExt == ".blend":
        bpy.ops.wm.open_mainfile(filepath=filePath)
    else:
        print("IMPORT FILEFORMAT NOT SUPPORTED")
        return
        
    bpy.ops.transform.rotate(
        value=radians(rotAmount),
        orient_axis=rotAxis)
    
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    if exportFormat == ".blend":
        bpy.ops.wm.save_mainfile(filepath=newfile)
    elif exportFormat == ".fbx":
        bpy.ops.export_scene.fbx(filepath=newfile, apply_unit_scale=True, use_space_transform=True, bake_space_transform=True)
    elif exportFormat == ".obj":
        bpy.ops.export_scene.obj(filepath=newfile)
    elif exportFormat == ".glb" or exportFormat == ".gltf":
        bpy.ops.export_scene.gltf(filepath=newfile)
    else:
        print("EXPORT FILEFORMAT NOT SUPPORTED")
        return



parseAndExport()

