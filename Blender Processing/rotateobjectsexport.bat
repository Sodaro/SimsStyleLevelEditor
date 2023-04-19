@echo off
set ScriptPath=%~dp0\rotationscript.py
set ExtensionFilter="*.fbx" "*.obj" "*.blend" "*.gltf" "*.glb"
set ExportFormat=""
set /p MyPath="Path: "
set /p ExtensionFilter="Extension Filter: (*.fbx, *.obj, *.blend, *.glb, *.gltf)"
set /p RotationAxis="RotationAxis: "
set /p RotationAmount="Rotation amount in degrees: "
set /p ExportFormat="Export format (.fbx, .obj, .blend, .glb, .gltf): "
cd /d %MyPath%
set DefaultExportPath=%cd%\Export
set ExportPath="%DefaultExportPath%"
set /p ExportPath="Export Path (default is origin directory/Export): "
if %ExportPath% == "%DefaultExportPath%" (
	echo Default Default
	mkdir Export
) else (
	echo Not Default
)
for %%f in (%ExtensionFilter%) do (
	blender -b -P "%ScriptPath%" -- "%cd%\%%f" %RotationAxis% %RotationAmount% %ExportPath% %ExportFormat%
)
pause