using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ResourceMapper : MonoBehaviour
{
    [MenuItem("Window/Generate Resource Asset")]
    static void GenerateResourceAsset()
    {
        Object[] objs = Resources.LoadAll(ProjectGlobals.ResourcePrefabsFolder);
        var folder = ProjectGlobals.ResourcePrefabsFolder;
        var data = new Dictionary<int, ResourceData>();
        foreach (Object obj in objs)
        {
            var newData = new ResourceData { ResourcePath = $"{folder}/{obj.name}.prefab", PrefabID = obj.GetInstanceID() };
            data[obj.GetInstanceID()] = newData;
        }
        var jsonstr = JsonConvert.SerializeObject(data);
        File.WriteAllText(ProjectGlobals.DataPath, jsonstr);
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
