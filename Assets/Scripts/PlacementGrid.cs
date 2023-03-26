using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    public delegate void OnObjectPlaced(GameObject prefab, GameObject instance);
    public event OnObjectPlaced onObjectPlaced;

    public enum PlacementRules { GridCenter, GridLines };

    [SerializeField] private GameObject _objectPrefab;
    [SerializeField] int _gridSize = 100;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _buildRooms = false;
    [SerializeField] private bool _deleteOverlappingObjects = false;
    [SerializeField] private PlacementRules _currentPlacementRules;
    [SerializeField] private GameSerializer _gameSerializer;
    Material lineMaterial;

    private (Vector3, Vector3)[] _gridVertices;


    private Vector3? _buildStartPoint = Vector3.zero;
    private Vector3? _buildEndPoint = Vector3.zero;


    Plane plane;
    int _currentHeight = 0;

    private Bounds _gridBounds;
    private void Awake()
    {
        CreateLineMaterial();
        plane = new Plane(Vector3.up, 0);
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(ray, out float distance)) { return false; }

        Vector3 worldPos = ray.GetPoint(distance);
        Vector3 desiredPosition = GridUtilities.GetTileCenterFromWorld(worldPos);
        if (!_gridBounds.Contains(desiredPosition)) { return false; }

        mousePosition = desiredPosition;
        return true;
    }

    public void Test()
    {
        foreach (var data in _gameSerializer.DataBase)
        {
            print(Resources.InstanceIDToObject(data.Value.PrefabID).name);
        }
    }

    public void SetObjectPrefab(int prefabID)
    {
        _objectPrefab = Resources.InstanceIDToObject(prefabID) as GameObject;
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
        switch (_currentPlacementRules)
        {
            case PlacementRules.GridCenter:
                TryConvertMouseToGridCenter(out _buildStartPoint);
                break;
            case PlacementRules.GridLines:
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
        var minZ = Mathf.Min(start.z, end.z);
        var maxZ = Mathf.Max(start.z, end.z);
        var minX = Mathf.Min(start.x, end.x);
        var maxX = Mathf.Max(start.x, end.x);
        var colliders = Physics.OverlapBox((start + end) / 2, new Vector3((maxX - minX) / 2, 10, (maxZ - minZ) / 2), Quaternion.identity, 1);
        foreach (var collider in colliders)
        {
            Destroy(collider.transform.parent.gameObject);
        }
    }

    private void FinishBuild()
    {
        switch (_currentPlacementRules)
        {
            case PlacementRules.GridCenter:
                TryConvertMouseToGridCenter(out _buildEndPoint);
                break;
            case PlacementRules.GridLines:
                TryConvertMouseToGrid(out _buildEndPoint);
                break;
        }

        if (_buildEndPoint != null && _buildStartPoint != null)
        {
            var start = _buildStartPoint.Value;
            var end = _buildEndPoint.Value;
            if (_buildRooms)
            {
                if (start.z == end.z || start.x == end.x)
                {
                    _buildEndPoint = null;
                    _buildStartPoint = null;
                    return;
                }

                var minZ = Mathf.Min(start.z, end.z);
                var maxZ = Mathf.Max(start.z, end.z);
                var minX = Mathf.Min(start.x, end.x);
                var maxX = Mathf.Max(start.x, end.x);
                var tl = new Vector3(minX, start.y, maxZ);
                var tr = new Vector3(maxX, start.y, maxZ);
                var bl = new Vector3(minX, start.y, minZ);
                var br = new Vector3(maxX, start.y, minZ);
                if (_deleteOverlappingObjects)
                {
                    DestroyObjectsInBox(start, end);
                }
                PlaceWall(tl, tr);
                PlaceWall(tr, br);
                PlaceWall(br, bl);
                PlaceWall(bl, tl);
            }
            else
            {
                PlaceWall(start, end);
            }

            _buildEndPoint = null;
            _buildStartPoint = null;
        }
    }

    void PlaceWall(Vector3 startPoint, Vector3 endPoint)
    {
        var diff = endPoint - startPoint;
        var count = Mathf.Max(diff.magnitude / GridUtilities.TileSize, 1);
        for (int i = 0; i < count; i++)
        {

            //TODO: Fix this so we don't fire 100 events, either package data or use other way of storing/sending data
            var rot = Quaternion.LookRotation(diff.normalized, Vector3.up);
            var instance = Instantiate(_objectPrefab, startPoint + diff.normalized * i * GridUtilities.TileSize, rot, transform.parent);
            onObjectPlaced.Invoke(_objectPrefab, instance);
        }
    }

    void CreateLineMaterial()
    {
        // simple colored things.
        var shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
    }

    void OnRenderObject()
    {
        lineMaterial.SetPass(0);
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

    public void SnapObjectToGrid(GameObject objectToSnap)
    {
        Vector3? mouseGridPosition;
        switch (_currentPlacementRules)
        {
            case PlacementRules.GridCenter:
                if (!TryConvertMouseToGridCenter(out mouseGridPosition))
                {
                    return;
                }
                objectToSnap.transform.position = mouseGridPosition.Value;
                break;
            case PlacementRules.GridLines:
                if (!TryConvertMouseToGrid(out mouseGridPosition))
                {
                    return;
                }
                objectToSnap.transform.position = mouseGridPosition.Value;
                break;
        }
    }

    private void Update()
    {
        SnapObjectToGrid(_targetTransform.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_gridBounds.center, _gridBounds.size);
    }
}
