using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSerializer : MonoBehaviour
{
    public Dictionary<int, ResourceData> DataBase;
    [SerializeField] private PlacementGrid _placementGrid;
    private Dictionary<int, GameInstanceData> _sceneData;

    private void Awake()
    {
        DataBase = JsonConvert.DeserializeObject<Dictionary<int, ResourceData>>(System.IO.File.ReadAllText(ProjectGlobals.DataPath));
        _sceneData = new Dictionary<int, GameInstanceData>();
        _placementGrid.onObjectPlaced += _placementGrid_onObjectPlaced;
    }

    private void _placementGrid_onObjectPlaced(GameObject prefab, GameObject instance)
    {
        _sceneData.Add(instance.GetInstanceID(), new GameInstanceData
        {
            ResourceData = DataBase[prefab.GetInstanceID()],
            InstancePosition = new(instance.transform.position),
            InstanceRotation = new(instance.transform.rotation),
            InstanceScale = new(instance.transform.localScale)
        });
    }

    private void Start()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        op.completed += (AsyncOperation result) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        };
    }
    public void SerializeSceneData()
    {
        var jsonstr = JsonConvert.SerializeObject(_sceneData);
        File.WriteAllText(ProjectGlobals.ResourcePath + "myscene.json", jsonstr);
    }
    public void DeserializeSceneData()
    {
        var data = JsonConvert.DeserializeObject<Dictionary<int, GameInstanceData>>(File.ReadAllText(ProjectGlobals.ResourcePath + "myscene.json"));
        _sceneData.Clear();
        foreach (var instanceData in data.Values)
        {
            Vector3 pos = instanceData.InstancePosition.ToVector3();
            Vector3 scale = instanceData.InstanceScale.ToVector3();
            Quaternion rot = instanceData.InstanceRotation.ToQuaternion();
            var instance = Instantiate(Resources.InstanceIDToObject(instanceData.ResourceData.PrefabID), pos, rot) as GameObject;
            instance.transform.localScale = scale;
            _sceneData.Add(instance.GetInstanceID(), instanceData);
        }
    }
}
