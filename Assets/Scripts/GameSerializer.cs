using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameSerializer : MonoBehaviour
{
    //public Dictionary<int, ResourceData> DataBase;

    public delegate void OnPlaceablesLoaded(Dictionary<string, PlaceableScriptableObject> pairs);
    public event OnPlaceablesLoaded onPlaceablesLoaded;


    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private TMP_InputField _inputField;
    private Dictionary<int, GameInstanceData> _sceneData;


    public Dictionary<string, PlaceableScriptableObject> PlaceableObjects = new Dictionary<string, PlaceableScriptableObject>();

    Scene _currentScene;

    string path;
    private IEnumerator PreloadPlaceables()
    {
        //find all the locations with label "placeable"
        var loadResourceLocationsHandle
            = Addressables.LoadResourceLocationsAsync("placeable", typeof(PlaceableScriptableObject));

        if (!loadResourceLocationsHandle.IsDone)
            yield return loadResourceLocationsHandle;

        //start each location loading
        List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationsHandle.Result)
        {
            AsyncOperationHandle<PlaceableScriptableObject> loadAssetHandle
                = Addressables.LoadAssetAsync<PlaceableScriptableObject>(location);
            loadAssetHandle.Completed +=
                obj => { PlaceableObjects.Add(location.PrimaryKey, obj.Result); };
            opList.Add(loadAssetHandle);
        }
        //create a GroupOperation to wait on all the above loads at once. 
        var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

        if (!groupOp.IsDone)
            yield return groupOp;

        Addressables.Release(loadResourceLocationsHandle);

        onPlaceablesLoaded.Invoke(PlaceableObjects);
    }


    private void Awake()
    {
#if UNITY_EDITOR
        path = Application.dataPath;
#else
        path = path = Application.persistentDataPath;
#endif
        //DataBase = JsonConvert.DeserializeObject<Dictionary<int, ResourceData>>(File.ReadAllText(ProjectGlobals.DataPath));
        _sceneData = new Dictionary<int, GameInstanceData>();
        _placementGrid.onObjectPlaced += OnObjectPlaced;
        _placementGrid.onObjectDeleted += OnObjectDeleted;
        _placementGrid.onObjectsPlaced += OnObjectsPlaced;
        _placementGrid.onObjectsDeleted += OnObjectsDeleted;
        StartCoroutine(PreloadPlaceables());
    }

    private void OnObjectsDeleted(List<int> instanceIDs)
    {
        foreach (int id in instanceIDs)
        {
            RemoveInstanceData(id);
        }
    }

    private void OnObjectsPlaced(GameObject prefab, List<GameObject> instances)
    {
        foreach (GameObject obj in instances)
        {
            StoreInstanceData(prefab, obj);
        }
    }

    private void RemoveInstanceData(int id)
    {
        if (!_sceneData.ContainsKey(id))
        {
            return;
        }
        _sceneData.Remove(id);
    }

    private void StoreInstanceData(GameObject prefab, GameObject instance)
    {
        _sceneData.Add(instance.GetInstanceID(), new GameInstanceData
        {
            InstancePosition = new(instance.transform.position),
            InstanceRotation = new(instance.transform.rotation),
            InstanceScale = new(instance.transform.localScale),
            AddressableKey = prefab.name,
        });
    }

    private void OnObjectDeleted(int instanceID)
    {
        RemoveInstanceData(instanceID);
    }

    private void OnObjectPlaced(GameObject prefab, GameObject instance)
    {
        StoreInstanceData(prefab, instance);
    }

    private void Start()
    {
        var op = Addressables.LoadSceneAsync("EmptyScene", LoadSceneMode.Additive);
        op.Completed += (AsyncOperationHandle<SceneInstance> handle) =>
        {
            _currentScene = handle.Result.Scene;
            SceneManager.SetActiveScene(_currentScene);
        };

    }

    //TODO: Move Save and Load functions to separate class
    public void SaveScene()
    {
        string name = _inputField.text;

        SerializeSceneData(_sceneData, Path.Combine(path, name + ".json"));
    }

    public void LoadScene()
    {
        List<GameObject> rootObjects = new List<GameObject>();
        _currentScene.GetRootGameObjects(rootObjects);

        // Destroy all gameobjects in current scene
        for (int i = 0; i < rootObjects.Count; ++i)
        {
            GameObject gameObject = rootObjects[i];
            Destroy(gameObject);
        }
        string name = _inputField.text;
        _sceneData = DeserializeSceneData(Path.Combine(path, name + ".json"));
    }

    public void SerializeSceneData(Dictionary<int, GameInstanceData> data, string path)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Formatting = Formatting.Indented;
        var jsonstr = JsonConvert.SerializeObject(data.Values, settings);
        File.WriteAllText(path, jsonstr);
    }

    public Dictionary<int, GameInstanceData> DeserializeSceneData(string path)
    {
        //Create a new dict as instance id is not the same when instantiating objects
        var deserializedData = JsonConvert.DeserializeObject<List<GameInstanceData>>(File.ReadAllText(path));
        var newData = new Dictionary<int, GameInstanceData>();
        foreach (var instanceData in deserializedData)
        {
            Vector3 pos = instanceData.InstancePosition.ToVector3();
            Vector3 scale = instanceData.InstanceScale.ToVector3();
            Quaternion rot = instanceData.InstanceRotation.ToQuaternion();
            var instance = Instantiate(PlaceableObjects[instanceData.AddressableKey].Prefab, pos, rot);
            instance.transform.localScale = scale;
            newData[instance.GetInstanceID()] = instanceData;
        }
        return newData;
    }
}
