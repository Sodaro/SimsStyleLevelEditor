using System.Collections;
using System.Collections.Generic;
using Unity.RuntimeSceneSerialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;

[System.Serializable]
public struct GameData
{
    public string ResourcePath;
    public int InstanceID;
}

public class GameSerializer : MonoBehaviour
{
    private void Start()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        op.completed += (AsyncOperation result) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
            Object[] objs = Resources.LoadAll("GamePrefabs");
            GameData data = new GameData();
            foreach (Object obj in objs)
            {
                //print(obj.GetInstanceID());
                //data.PrefabID = obj.GetInstanceID();
                //print(JsonUtility.ToJson(obj, true));

            }

            Object instance = Instantiate(objs[0]);
            print($"i:{instance.GetInstanceID()}, o:{objs[0].GetInstanceID()}");
            //instance = Instantiate(objs[0]) as GameObject;
            //print($"instance:{instance.GetInstanceID()}, obj[0]:{objs[0].GetInstanceID()}");
            //instance = Instantiate(objwall) as GameObject;
            //instance = Instantiate(objwall) as GameObject;
            //instance = Instantiate(objwall) as GameObject;
        };
    }
    public void SerializeScene()
    {

    }
    public void DeserializeScene()
    {


    }
}
