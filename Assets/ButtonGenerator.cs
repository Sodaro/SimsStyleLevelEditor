using UnityEngine;
using UnityEngine.UI;

public class ButtonGenerator : MonoBehaviour
{
    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private GameSerializer _gameSerializer;
    [SerializeField] private Button _buttonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var data in _gameSerializer.DataBase.Values)
        {
            var button = Instantiate(_buttonPrefab, transform);
            button.onClick.AddListener(delegate { _placementGrid.SetObjectPrefab(data.PrefabID); });
        }
    }
}
