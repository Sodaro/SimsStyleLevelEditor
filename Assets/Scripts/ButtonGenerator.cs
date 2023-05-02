using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

public class ButtonGenerator : MonoBehaviour
{
    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private GameSerializer _gameSerializer;
    [SerializeField] private Button _buttonPrefab;


    private Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();
    // Start is called before the first frame update
    private void Awake()
    {
        _gameSerializer.onPlaceablesLoaded += _gameSerializer_onPlaceablesLoaded;
    }

    private void GenerateButtons(System.Collections.Generic.Dictionary<string, PlaceableScriptableObject> pairs)
    {
        ////find all the locations with label "placeable"
        //var loadResourceLocationsHandle
        //    = Addressables.LoadResourceLocationsAsync("icon", typeof(Sprite));

        //if (!loadResourceLocationsHandle.IsDone)
        //    yield return loadResourceLocationsHandle;

        ////start each location loading
        //List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

        //foreach (IResourceLocation location in loadResourceLocationsHandle.Result)
        //{
        //    AsyncOperationHandle<Sprite> loadAssetHandle
        //        = Addressables.LoadAssetAsync<Sprite>(location);
        //    loadAssetHandle.Completed +=
        //        obj =>
        //        {
        //            _icons.Add(location.PrimaryKey, obj.Result);
        //        };
        //    opList.Add(loadAssetHandle);
        //}
        ////create a GroupOperation to wait on all the above loads at once. 
        //var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

        //if (!groupOp.IsDone)
        //    yield return groupOp;

        //Addressables.Release(loadResourceLocationsHandle);

        foreach (var data in pairs)
        {
            var button = Instantiate(_buttonPrefab, transform);
            button.onClick.AddListener(delegate
            {
                _placementGrid.SetObjectPrefab((data.Key, data.Value.Prefab));
            });
            button.GetComponent<Image>().sprite = data.Value.Sprite;
        }
    }

    private void _gameSerializer_onPlaceablesLoaded(System.Collections.Generic.Dictionary<string, PlaceableScriptableObject> pairs)
    {
        GenerateButtons(pairs);
        _gameSerializer.onPlaceablesLoaded -= _gameSerializer_onPlaceablesLoaded;
    }
}
