using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlaceableAddressableGenerator : MonoBehaviour
{
    [MenuItem("PlaceableAddressableGenerator/Generate Placeables")]
    static async void ExtractPreviewImages()
    {
        string prefabDirectory = EditorUtility.OpenFolderPanel("Select prefab directory", Application.dataPath, "");
        if (string.IsNullOrEmpty(prefabDirectory))
        {
            return;
        }

        string extractionPath = EditorUtility.OpenFolderPanel("Preview-Image destination folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(extractionPath))
        {
            return;
        }

        string scriptablePath = EditorUtility.OpenFolderPanel("Select Folder to store ScriptableObjects in", Application.dataPath, "");
        if (string.IsNullOrEmpty(scriptablePath))
        {
            return;
        }

        var assetPath = "Assets" + prefabDirectory.Replace(Application.dataPath, "");
        string[] guids = AssetDatabase.FindAssets("", new[] { assetPath });
        var data = await FetchPreviews(guids, extractionPath);
        GeneratePlaceables(data, scriptablePath);
    }

    static async Task<List<(string, string)>> FetchPreviews(string[] guids, string extractionPath)
    {
        var objectSpritePaths = new List<(string, string)>();
        int progressID = Progress.Start("Running Image Extractor...");
        for (int i = 0; i < guids.Length; i++)
        {
            var str = guids[i];
            var tempPath = AssetDatabase.GUIDToAssetPath(str);
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(tempPath);
            var texture = AssetPreview.GetAssetPreview(obj);
            Progress.Report(progressID, (float)i / guids.Length, $"Extracting asset image: {obj.name}");
            int max = 1000;
            int count = 0;
            while (texture == null && count < max)
            {
                texture = AssetPreview.GetAssetPreview(obj);
                count++;
                await System.Threading.Tasks.Task.Yield();
            }
            if (texture == null)
                continue;
            var bytes = texture.EncodeToPNG();
            var objPath = extractionPath + "/" + obj.name + ".png";
            File.WriteAllBytes(objPath, bytes);
            var assetpath = "Assets" + objPath.Replace(Application.dataPath, "");
            AssetDatabase.ImportAsset(assetpath);
            TextureImporter importer = AssetImporter.GetAtPath(assetpath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            AssetDatabase.WriteImportSettingsIfDirty(assetpath);

            objectSpritePaths.Add((tempPath, assetpath));
        }
        
        Progress.Remove(progressID);
        AssetDatabase.Refresh();
        return objectSpritePaths;
    }

    static void GeneratePlaceables(List<(string, string)> objectSpritePaths, string scriptableObjPath)
    {
        foreach (var objectSpritePath in objectSpritePaths)
        {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(objectSpritePath.Item1);
            var scriptableObjAssetPath = $"Assets/{scriptableObjPath.Replace(Application.dataPath, "")}/{obj.name}.asset";
            var scriptableObj = ScriptableObject.CreateInstance<PlaceableScriptableObject>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(objectSpritePath.Item2);
            scriptableObj.Sprite = sprite;
            scriptableObj.Prefab = obj;
            AssetDatabase.CreateAsset(scriptableObj, scriptableObjAssetPath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("PlaceableAddressableGenerator/Make files in folder addressable")]
    static void AddPlaceablesToAddressableGroup()
    {
        string targetPath = EditorUtility.OpenFolderPanel("Select Folder with files to make addressable", Application.dataPath, "");
        if (string.IsNullOrEmpty(targetPath))
        {
            return;
        }
        var label = EditorInputDialog.Show("Label", "Enter Addressable Label", "");
        AddAssetsToAddressables(targetPath, label);
    }
    static void AddAssetsToAddressables(string assetPath, string label)
    {
        assetPath = "Assets" + assetPath.Replace(Application.dataPath, "");
        string[] guids = AssetDatabase.FindAssets("", new[] { assetPath });
        foreach (var guid in guids)
        {
            AddAssetToAddressables(guid);
            SetLabelForAddressables(guid, label);
        }
    }
    public static AssetReference AddAssetToAddressables(string assetguid)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        return settings.CreateAssetReference(assetguid);
    }
    public static void SetLabelForAddressables(string assetguid, string label)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetEntry entry = settings.FindAssetEntry(assetguid);
        entry.SetLabel(label, true);
        entry.SetAddress(Path.GetFileNameWithoutExtension(entry.address));
    }
    public static AssetReference AddAssetToAddressables(UnityEngine.Object asset, string label)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry entry = settings.FindAssetEntry(assetGUID);
        entry.SetLabel(label, true);
        return settings.CreateAssetReference(assetGUID);
    }
}
