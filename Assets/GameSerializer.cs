using System.Collections;
using System.Collections.Generic;
using Unity.RuntimeSceneSerialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;

public class GameSerializer : MonoBehaviour
{
    private void Start()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        op.completed += (AsyncOperation result) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        };
    }
    public void SerializeScene()
    {
        var activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
            return;

        var path = Application.dataPath + "/myscene.json";

        var assetPackPath = Path.ChangeExtension(path, ".asset");
        assetPackPath = assetPackPath.Replace(Application.dataPath, "Assets");

        var assetPack = AssetDatabase.LoadAssetAtPath<AssetPack>(assetPackPath);
        var created = false;
        if (assetPack == null)
        {
            created = true;
            assetPack = ScriptableObject.CreateInstance<AssetPack>();
        }
        else
        {
            assetPack.Clear();
        }

        var renderSettings = SerializedRenderSettings.CreateFromActiveScene();
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene.path);
        if (sceneAsset != null)
            assetPack.SceneAsset = sceneAsset;

        File.WriteAllText(path, SceneSerialization.SerializeScene(activeScene, renderSettings, assetPack));

        if (created)
        {
            //if (assetPack.AssetCount > 0)
            AssetDatabase.CreateAsset(assetPack, assetPackPath);
        }
        else
        {
            if (assetPack.AssetCount > 0)
                EditorUtility.SetDirty(assetPack);
            else if (AssetDatabase.LoadAssetAtPath<AssetPack>(assetPackPath) != null)
                AssetDatabase.DeleteAsset(assetPackPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //var activeScene = SceneManager.GetActiveScene();
        //string jsonText = SceneSerialization.SerializeScene(SceneManager.GetActiveScene());
        //File.WriteAllText("Assets/testfile.json", jsonText);
    }
    public void DeserializeScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
        op.completed += (AsyncOperation result) =>
        {
            AsyncOperation op2 = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

            op2.completed += (AsyncOperation result) =>
            {
                var path = Application.dataPath + "/myscene.json";
                var assetPackPath = Path.ChangeExtension(path, ".asset");
                assetPackPath = assetPackPath.Replace(Application.dataPath, "Assets");
                var assetPack = AssetDatabase.LoadAssetAtPath<AssetPack>(assetPackPath);
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(2));
                string jsonText = File.ReadAllText("Assets/myscene.json");
                SceneSerialization.ImportScene(jsonText, assetPack);
            };
        };

    }
}
