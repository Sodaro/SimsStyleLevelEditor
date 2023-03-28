using UnityEngine;
using UnityEngine.UI;

public class ButtonGenerator : MonoBehaviour
{
    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private GameSerializer _gameSerializer;
    [SerializeField] private Button _buttonPrefab;
    // Start is called before the first frame update
    private void Awake()
    {
        _gameSerializer.onPlaceablesLoaded += _gameSerializer_onPlaceablesLoaded;
    }

    private void _gameSerializer_onPlaceablesLoaded(System.Collections.Generic.Dictionary<string, GameObject> pairs)
    {
        foreach (var data in pairs)
        {
            var button = Instantiate(_buttonPrefab, transform);
            button.onClick.AddListener(delegate { _placementGrid.SetObjectPrefab((data.Key, data.Value)); });
        }
        _gameSerializer.onPlaceablesLoaded -= _gameSerializer_onPlaceablesLoaded;
    }
}
