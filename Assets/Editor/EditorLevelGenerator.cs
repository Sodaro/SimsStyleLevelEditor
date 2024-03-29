using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EditorLevelGenerator : MonoBehaviour
{
    [MenuItem("LevelEditor/Generate Level From Data JSON")]
    static void LoadLevelData()
    {
        string path = EditorUtility.OpenFilePanel("Select level json", "", "json");
        if (path.Length == 0)
        {
            return;
        }
        string jsontext = File.ReadAllText(path);
        if (jsontext.Length == 0)
        {
            return;
        }
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Load Level Data");
        int group = Undo.GetCurrentGroup();
        var data = JsonConvert.DeserializeObject<Dictionary<int, GameInstanceData>>(jsontext);
        foreach (var instanceData in data.Values)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(instanceData.AddressableKey);
            op.WaitForCompletion();
            op.Completed += (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj) =>
            {
                Vector3 pos = instanceData.InstancePosition.ToVector3();
                Vector3 scale = instanceData.InstanceScale.ToVector3();
                Quaternion rot = instanceData.InstanceRotation.ToQuaternion();
                var instance = PrefabUtility.InstantiatePrefab(obj.Result) as GameObject;
                instance.transform.position = pos;
                instance.transform.rotation = rot;
                instance.transform.localScale = scale;
                Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            };

        }
        Undo.CollapseUndoOperations(group);
    }

    private static void Op_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {
        throw new System.NotImplementedException();
    }
}
