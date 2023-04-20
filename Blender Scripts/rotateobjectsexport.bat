@echo off
echo =======================================
echo BLENDER MODEL ROTATOR  
echo =======================================
set ScriptPath=%~dp0\rotationscript.py
set ExtensionFilter="*.fbx" "*.obj" "*.blend" "*.gltf" "*.glb"
set ExportFormat=""
set /p ImportDirectory="Import Directory: "
set /p ExtensionFilter="Extension Filter (*.fbx, *.obj, *.blend, *.glb, *.gltf): "
choice /c xyz /n /m "Which Rotation Axis (X, Y, Z)?: "
if errorlevel 1 set RotationAxis=X
if errorlevel 2 set RotationAxis=Y
if errorlevel 3 set RotationAxis=Z
set /p RotationAmount="Rotation amount in degrees: "
set /p ExportFormat="Export format (.fbx, .obj, .blend, .glb, .gltf): "
cd /d %ImportDirectory%
set DefaultExportPath=%cd%\Export
set ExportPath="%DefaultExportPath%"
set /p ExportPath="Output Directory (default is origin directory/Export): "
if %ExportPath% == "%DefaultExportPath%" (
	echo Default Default
	mkdir Export
) else (
	echo Not Default
)
blender -b -P "%ScriptPath%" -- "%cd%" %RotationAxis% %RotationAmount% %ExportPath% %ExportFormat%
pause