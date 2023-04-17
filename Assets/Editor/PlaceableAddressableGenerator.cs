using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlaceableAddressableGenerator : MonoBehaviour
{
    [MenuItem("PlaceableAddressableGenerator/Extract preview images from folder")]
    static void ExtractPreviewImages()
    {
        string extractionPath = EditorUtility.OpenFolderPanel("Select Folder to extract from", Application.dataPath, "");
        string targetPath = EditorUtility.OpenFolderPanel("Select Folder to extract to", Application.dataPath, "");
        var assetPath = "Assets" + extractionPath.Replace(Application.dataPath, "");
        string[] guids = AssetDatabase.FindAssets("", new[] { assetPath });
        foreach (string str in guids)
        {
            var tempPath = AssetDatabase.GUIDToAssetPath(str);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(tempPath);
            var texture = AssetPreview.GetAssetPreview(obj);
            if (texture == null)
                continue;
            var bytes = texture.EncodeToPNG();
            var objPath = targetPath + "/" + obj.name + ".png";
            File.WriteAllBytes(objPath, bytes);
            var assetpath = "Assets" + objPath.Replace(Application.dataPath, "");
            AssetDatabase.ImportAsset(assetpath);
            TextureImporter importer = AssetImporter.GetAtPath(assetpath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            AssetDatabase.WriteImportSettingsIfDirty(assetpath);
        }
        AddAssetsToAddressables(targetPath, "icon");
        AssetDatabase.Refresh();
    }
    [MenuItem("PlaceableAddressableGenerator/Add placeables to addressable group")]
    static void AddPlaceablesToAddressableGroup()
    {
        string targetPath = EditorUtility.OpenFolderPanel("Select Folder to extract to", Application.dataPath, "");
        AddAssetsToAddressables(targetPath, "placeable");
    }
    static void AddAssetsToAddressables(string assetPath, string label)
    {
        assetPath = "Assets" + assetPath.Replace(Application.dataPath, "");
        string[] guids = AssetDatabase.FindAssets("", new[] { assetPath });
        foreach (var guid in guids)
        {
            var asset = AddAssetToAddressables(guid);
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
    public static AssetReference AddAssetToAddressables(Object asset, string label)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry entry = settings.FindAssetEntry(assetGUID);
        entry.SetLabel(label, true);
        return settings.CreateAssetReference(assetGUID);
    }
}
