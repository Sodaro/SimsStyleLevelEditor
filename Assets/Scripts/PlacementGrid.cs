using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public struct BuildOptions
{
    public enum PlacementRules { GridCenter, GridLines };
    public enum PlacementMode { Line, HollowRectangle, FilledRectangle};
    public PlacementMode ActivePlacementMode;
    public PlacementRules ActivePlacementRules;
    public bool DeleteOverlappingObjects;
}

public class PlacementGrid : MonoBehaviour
{
    public delegate void OnObjectPlaced(GameObject prefab, GameObject instance);
    public delegate void OnObjectsPlaced(GameObject prefab, List<GameObject> instances);
    public delegate void OnObjectDeleted(int instanceID);
    public delegate void OnObjectsDeleted(List<int> instanceIDs);
    public event OnObjectPlaced onObjectPlaced;
    public event OnObjectsPlaced onObjectsPlaced;
    public event OnObjectDeleted onObjectDeleted;
    public event OnObjectsDeleted onObjectsDeleted;

    [SerializeField] private Toolbar _toolbar;
    [SerializeField] private int _gridSize = 100;
    [SerializeField] private GameSerializer _gameSerializer;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private GameObject _buildPreview;

    private (Vector3, Vector3)[] _gridVertices;
    private (string, GameObject) _selectedAddressable;
    private BuildOptions _buildOptions;
    private Vector3? _buildStartPoint = null;
    private Vector3? _buildEndPoint = null;
    private Vector3 _mouseGridPosition = Vector3.zero;
    private Plane plane;
    private int _currentHeight = 0;
    private Bounds _gridBounds;

    private void CreateGrid()
    {
        int sideLength = _gridSize + 1;
        _gridVertices = new (Vector3, Vector3)[sideLength * 2];
        int lineLength = _gridSize * GridUtilities.TileSize;
        int index = 0;
        var initialOffset = -(lineLength / 2);

        //rows
        for (int i = 0; i < sideLength; i++, index++)
        {
            int lineOffset = i * GridUtilities.TileSize;
            _gridVertices[index] = (new Vector3(initialOffset, 0, initialOffset + lineOffset), new Vector3(initialOffset + lineLength, 0, initialOffset + lineOffset));
        }

        //columns
        for (int i = 0; i < sideLength; i++, index++)
        {
            int lineOffset = i * GridUtilities.TileSize;
            _gridVertices[index] = (new Vector3(initialOffset + lineOffset, 0, initialOffset), new Vector3(initialOffset + lineOffset, 0, initialOffset + lineLength));
        }
        _gridBounds = new Bounds(Vector3.zero, new Vector3(lineLength, lineLength, lineLength));
    }
    private void Awake()
    {
        plane = new Plane(Vector3.up, 0);
        CreateGrid();

        _gameSerializer.onPlaceablesLoaded += _gameSerializer_onPlaceablesLoaded;
        var options = Enum.GetNames(typeof(BuildOptions.PlacementRules)).ToList();
        _toolbar.OptionsDropdown.AddOptions(options);
    }

    private void OnOptionsDropdownValueChanged(int value) => _buildOptions.ActivePlacementRules = (BuildOptions.PlacementRules)value;
    private void OnHollowRectanglePlacementToggleValueChanged(bool value)
    {
        if (value)
        {
            _buildOptions.ActivePlacementMode = BuildOptions.PlacementMode.HollowRectangle;
        }
    }
    private void OnFilledRectanglePlacementToggleValueChanged(bool value)
    {
        if (value)
        {
            _buildOptions.ActivePlacementMode = BuildOptions.PlacementMode.FilledRectangle;
        }
    }
    private void OnLinePlacementToggleValueChanged(bool value)
    {
        if (value)
        {
            _buildOptions.ActivePlacementMode = BuildOptions.PlacementMode.Line;
        }
    }

    private void OnDeleteOverlappingToggleValueChanged(bool value) => _buildOptions.DeleteOverlappingObjects = value;

    private void OnEnable()
    {
        _toolbar.OptionsDropdown.onValueChanged.AddListener(OnOptionsDropdownValueChanged);
        _toolbar.PlaceLineToggle.onValueChanged.AddListener(OnLinePlacementToggleValueChanged);
        _toolbar.PlaceHollowRectangleToggle.onValueChanged.AddListener(OnHollowRectanglePlacementToggleValueChanged);
        _toolbar.PlaceFilledRectangleToggle.onValueChanged.AddListener(OnFilledRectanglePlacementToggleValueChanged);
        _toolbar.DeleteOverlapToggle.onValueChanged.AddListener(OnDeleteOverlappingToggleValueChanged);
    }



    private void OnDisable()
    {
        _toolbar.OptionsDropdown.onValueChanged.RemoveListener(OnOptionsDropdownValueChanged);
        _toolbar.PlaceHollowRectangleToggle.onValueChanged.RemoveListener(OnHollowRectanglePlacementToggleValueChanged);
        _toolbar.PlaceFilledRectangleToggle.onValueChanged.RemoveListener(OnFilledRectanglePlacementToggleValueChanged);
        _toolbar.DeleteOverlapToggle.onValueChanged.RemoveListener(OnDeleteOverlappingToggleValueChanged);
    }

    private void _gameSerializer_onPlaceablesLoaded(System.Collections.Generic.Dictionary<string, PlaceableScriptableObject> pairs)
    {
        var pair = pairs.ElementAt(0);
        _selectedAddressable = (pair.Key, pair.Value.Prefab);
    }

    private bool TryConvertMouseToGrid(out Vector3? mousePosition)
    {
        mousePosition = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(ray, out float distance)) { return false; }

        Vector3 worldPos = ray.GetPoint(distance);
        Vector3 desiredPosition = GridUtilities.GetGridPosition(worldPos);
        if (!_gridBounds.Contains(desiredPosition)) { return false; }

        mousePosition = desiredPosition;
        return true;
    }
    private bool TryConvertMouseToGridCenter(out Vector3? mousePosition)
    {
        mousePosition = null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!plane.Raycast(ray, out float distance)) { return false; }

        Vector3 worldPos = ray.GetPoint(distance);
        Vector3 desiredPosition = GridUtilities.GetTileCenterFromWorld(worldPos);
        if (!_gridBounds.Contains(desiredPosition)) { return false; }

        mousePosition = desiredPosition;
        return true;
    }

    public void SetObjectPrefab((string, GameObject) addressable)
    {
        _selectedAddressable = addressable;
    }
    public void HandleMouseClick(Player.MouseClickType clickType)
    {
        if (clickType == Player.MouseClickType.Pressed)
        {
            StartBuild();
        }
        else if (clickType == Player.MouseClickType.Released)
        {
            FinishBuild();
        }
    }

    private void StartBuild()
    {
        switch (_buildOptions.ActivePlacementRules)
        {
            case BuildOptions.PlacementRules.GridCenter:
                TryConvertMouseToGridCenter(out _buildStartPoint);
                break;
            case BuildOptions.PlacementRules.GridLines:
                TryConvertMouseToGrid(out _buildStartPoint);
                break;
        }
    }

    public void IncreasePlaneHeight()
    {
        _currentHeight++;
        plane.SetNormalAndPosition(Vector3.up, Vector3.up * _currentHeight);
    }
    public void DecreasePlaneHeight()
    {
        _currentHeight--;
        plane.SetNormalAndPosition(Vector3.up, Vector3.up * _currentHeight);
    }

    public void DestroyObjectsInBox(Vector3 start, Vector3 end)
    {
        //TODO: Replace Physics overlap with gridbased, or at least account for height
        var minZ = Mathf.Min(start.z, end.z);
        var maxZ = Mathf.Max(start.z, end.z);
        var minX = Mathf.Min(start.x, end.x);
        var maxX = Mathf.Max(start.x, end.x);
        var colliders = Physics.OverlapBox((start + end) / 2, new Vector3((maxX - minX) / 2, 10, (maxZ - minZ) / 2), Quaternion.identity, 1);
        var destroyedList = new List<int>();
        foreach (var collider in colliders)
        {
            var objToDestroy = collider.transform.gameObject;
            destroyedList.Add(objToDestroy.GetInstanceID());
            Destroy(objToDestroy);
        }
        onObjectsDeleted.Invoke(destroyedList);
    }

    private void FinishBuild()
    {
        switch (_buildOptions.ActivePlacementRules)
        {
            case BuildOptions.PlacementRules.GridCenter:
                TryConvertMouseToGridCenter(out _buildEndPoint);
                break;
            case BuildOptions.PlacementRules.GridLines:
                TryConvertMouseToGrid(out _buildEndPoint);
                break;
        }

        if (_buildEndPoint == null || _buildStartPoint == null)
        {
            return;
        }
        var start = _buildStartPoint.Value;
        var end = _buildEndPoint.Value;
        var placedObjects = new List<GameObject>();
        switch (_buildOptions.ActivePlacementMode)
        {
            case BuildOptions.PlacementMode.HollowRectangle:
                PlaceHollowRectangle(start, end, ref placedObjects);
                break;
            case BuildOptions.PlacementMode.FilledRectangle:
                PlaceFilledRectangle(start, end, ref placedObjects);
                break;
            default:
                PlaceRow(start, end, ref placedObjects);
                break;
        }
        if (placedObjects.Count > 0)
        {
            onObjectsPlaced.Invoke(_selectedAddressable.Item2, placedObjects);
        }
        _buildEndPoint = null;
        _buildStartPoint = null;
        _buildPreview.SetActive(false);
    }
    private void PlaceHollowRectangle(Vector3 startPoint, Vector3 endPoint, ref List<GameObject> placedObjects)
    {
        if (startPoint.z == endPoint.z || startPoint.x == endPoint.x)
        {
            _buildEndPoint = null;
            _buildStartPoint = null;
            return;
        }
        if (_buildOptions.DeleteOverlappingObjects)
        {
            DestroyObjectsInBox(startPoint, endPoint);
        }

        var minZ = Mathf.Min(startPoint.z, endPoint.z);
        var maxZ = Mathf.Max(startPoint.z, endPoint.z);
        var minX = Mathf.Min(startPoint.x, endPoint.x);
        var maxX = Mathf.Max(startPoint.x, endPoint.x);
        var tl = new Vector3(minX, startPoint.y, maxZ);
        var tr = new Vector3(maxX, startPoint.y, maxZ);
        var bl = new Vector3(minX, startPoint.y, minZ);
        var br = new Vector3(maxX, startPoint.y, minZ);

        PlaceRow(tl, tr, ref placedObjects);
        PlaceRow(tr, br, ref placedObjects);
        PlaceRow(br, bl, ref placedObjects);
        PlaceRow(bl, tl, ref placedObjects);
    }
    private void PlaceFilledRectangle(Vector3 startPoint, Vector3 endPoint, ref List<GameObject> placedObjects)
    {
        if (startPoint.z == endPoint.z || startPoint.x == endPoint.x)
        {
            _buildEndPoint = null;
            _buildStartPoint = null;
            return;
        }
        if (_buildOptions.DeleteOverlappingObjects)
        {
            DestroyObjectsInBox(startPoint, endPoint);
        }
        var dir = Mathf.Sign(endPoint.z - startPoint.z);
        int count = (int)Mathf.Abs(startPoint.z - endPoint.z);
        for (int i = 0; i < count; i++)
        {
            var rowStart = new Vector3(startPoint.x, startPoint.y, startPoint.z + i * GridUtilities.TileSize * dir);
            var rowEnd = new Vector3(endPoint.x, startPoint.y, startPoint.z + i * GridUtilities.TileSize * dir);
            PlaceRow(rowStart, rowEnd, ref placedObjects);
        }
    }

    private void PlaceRow(Vector3 startPoint, Vector3 endPoint, ref List<GameObject> placedObjects)
    {
        var diff = endPoint - startPoint;
        var count = Mathf.Max(diff.magnitude / GridUtilities.TileSize, 1);
        for (int i = 0; i < count; i++)
        {
            var rot = Quaternion.LookRotation(diff.normalized, Vector3.up);
            var instance = Instantiate(_selectedAddressable.Item2, startPoint + diff.normalized * i * GridUtilities.TileSize, rot, transform.parent);
            placedObjects.Add(instance);   
        }
    }

    private void OnRenderObject()
    {
        _lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix); // not needed if already in worldspace
        GL.Begin(GL.LINES);
        GL.Color(Color.cyan);
        for (int i = 0; i < _gridVertices.Length; i++)
        {
            var pair = _gridVertices[i];
            GL.Vertex(pair.Item1);
            GL.Vertex(pair.Item2);
        }

        GL.End();
        GL.PopMatrix();
    }

    public Vector3? GetMouseGridPosition()
    {
        Vector3? mouseGridPosition = Vector3.zero;
        switch (_buildOptions.ActivePlacementRules)
        {
            case BuildOptions.PlacementRules.GridCenter:
                TryConvertMouseToGridCenter(out mouseGridPosition);
                break;
            case BuildOptions.PlacementRules.GridLines:
                TryConvertMouseToGrid(out mouseGridPosition);
                break;
        }
        return mouseGridPosition;
    }

    public void SnapObjectToGrid(GameObject targetObject)
    {
        targetObject.transform.position = _mouseGridPosition;
    }

    private void UpdateBuildPreview()
    {
        if (_buildStartPoint == null)
            return;

        _buildPreview.SetActive(true);

        var diff = _mouseGridPosition - _buildStartPoint.Value;
        _buildPreview.transform.position = (_buildStartPoint.Value + _mouseGridPosition) / 2;
        if (_buildOptions.ActivePlacementMode  == BuildOptions.PlacementMode.HollowRectangle || _buildOptions.ActivePlacementMode == BuildOptions.PlacementMode.FilledRectangle)
        {
            var x = Mathf.Abs(diff.x);
            var y = Mathf.Max(1, Mathf.Abs(diff.y));
            var z = Mathf.Abs(diff.z);
            _buildPreview.transform.localScale = new Vector3(x, y, z);
            _buildPreview.transform.rotation = Quaternion.identity;
        }
        else
        {
            var rot = Quaternion.LookRotation(diff.normalized, Vector3.up);
            _buildPreview.transform.localScale = new Vector3(1, 1, diff.magnitude / 10);
            _buildPreview.transform.rotation = rot;
        }
    }

    private void Update()
    {
        var pos = GetMouseGridPosition();
        if (pos == null)
            return;

        _mouseGridPosition = pos.Value;
        UpdateBuildPreview();
        //SnapObjectToGrid(_targetTransform.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_gridBounds.center, _gridBounds.size);
    }
}
