import bpy
import os
import sys
from math import radians


def parseAndExport():
    def handleFile(path):
        rotAxis = sys.argv[1]
        rotAmount = float(sys.argv[2])
        exportPath = sys.argv[3]
        exportFormat = sys.argv[4]
        file = os.path.splitext(os.path.basename(path))
        fileName, fileExt = file[0], file[1]

        if exportFormat == "":
            exportFormat = fileExt

        newfile = os.path.join(exportPath, fileName + exportFormat)
        match fileExt:
            case ".fbx":
                bpy.ops.scene.new(type="NEW")
                bpy.ops.import_scene.fbx(filepath=path)
            case ".obj":
                bpy.ops.scene.new(type="NEW")
                bpy.ops.import_scene.obj(filepath=path)
            case ".glb"|".gltf":
                bpy.ops.scene.new(type="NEW")
                bpy.ops.import_scene.gltf(filepath=path)
            case ".blend":
                bpy.ops.wm.open_mainfile(filepath=path)
            case _:
                print("IMPORT FILEFORMAT NOT SUPPORTED, format was:", fileExt)
                return
            
        bpy.ops.transform.rotate(
            value=radians(rotAmount),
            orient_axis=rotAxis)
        
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        match exportFormat:
            case ".blend":
                bpy.ops.wm.save_mainfile(filepath=newfile)
            case ".fbx":
                bpy.ops.export_scene.fbx(filepath=newfile, apply_unit_scale=True, use_space_transform=True, bake_space_transform=True)
                bpy.ops.scene.delete()
            case ".obj":
                bpy.ops.export_scene.obj(filepath=newfile)
                bpy.ops.scene.delete()
            case ".glb"|".gltf":
                bpy.ops.export_scene.gltf(filepath=newfile)
                bpy.ops.scene.delete()
            case _:
                print("EXPORT FILEFORMAT NOT SUPPORTED")
        


    dirPath = sys.argv[0]
    # Iterate directory
    for path in os.listdir(dirPath):
        # check if current path is a file
        filePath = os.path.join(dirPath, path)
        if os.path.isfile(filePath):
            handleFile(filePath)

sys.argv = sys.argv[sys.argv.index("--") + 1:]  # get all args after "--"
parseAndExport()

