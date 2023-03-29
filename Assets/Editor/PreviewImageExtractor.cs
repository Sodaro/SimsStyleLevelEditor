using UnityEditor;
using UnityEngine;
public class PreviewImageExtractor : MonoBehaviour
{
    [MenuItem("AssetExtractor/Extract preview images from folder")]
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
            System.IO.File.WriteAllBytes(objPath, bytes);
        }
    }
}
