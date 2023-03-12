using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] int _gridSize = 100;
    Material lineMaterial;

    private (Vector3, Vector3)[] _gridVertices;

    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _snapPreviewToCenter = false;
    [SerializeField] private bool _buildRooms = false;


    private Vector3? _buildStartPoint = Vector3.zero;
    private Vector3? _buildEndPoint = Vector3.zero;

    Plane plane;

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
    bool TryConvertMouseToGrid(out Vector3? mousePosition)
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
    bool TryConvertMouseToGridCenter(out Vector3? mousePosition)
    {
        mousePosition = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(ray, out float distance)) { return false; }

        Vector3 worldPos = ray.GetPoint(distance);
        Vector3 desiredPosition = GridUtilities.GetTileCenterFromWorldXZ(worldPos);
        if (!_gridBounds.Contains(desiredPosition)) { return false; }

        mousePosition = desiredPosition;
        return true;
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
        TryConvertMouseToGrid(out _buildStartPoint);
    }

    private void FinishBuild()
    {
        if (!TryConvertMouseToGrid(out _buildEndPoint)) { return; }

        if (_buildEndPoint != null && _buildStartPoint != null)
        {
            if (_buildRooms)
            {
                var start = _buildStartPoint.Value;
                var end = _buildEndPoint.Value;
                var minZ = Mathf.Min(start.z, end.z);
                var maxZ = Mathf.Max(start.z, end.z);
                var minX = Mathf.Min(start.x, end.x);
                var maxX = Mathf.Max(start.x, end.x);
                var tl = new Vector3(minX, start.y, maxZ);
                var tr = new Vector3(maxX, start.y, maxZ);
                var bl = new Vector3(minX, start.y, minZ);
                var br = new Vector3(maxX, start.y, minZ);
                PlaceWall(tl, tr);
                PlaceWall(tr, br);
                PlaceWall(br, bl);
                PlaceWall(bl, tl);
            }
            else
            {
                PlaceWall(_buildStartPoint.Value, _buildEndPoint.Value);
            }

            _buildEndPoint = null;
            _buildStartPoint = null;
        }
    }

    void PlaceWall(Vector3 startPoint, Vector3 endPoint)
    {
        var diff = endPoint - startPoint;
        var count = diff.magnitude / GridUtilities.TileSize;
        for (int i = 0; i < count; i++)
        {
            var rot = Quaternion.LookRotation(diff.normalized, Vector3.up);
            var instance = Instantiate(_wallPrefab, startPoint + diff.normalized * i * GridUtilities.TileSize, rot, transform.parent);
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
        GL.Color(Color.red);
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
        Vector3? mouseGridPosition = null;
        if (!TryConvertMouseToGridCenter(out mouseGridPosition))
        {
            return;
        }
        objectToSnap.transform.position = mouseGridPosition.Value;
    }

    private void Update()
    {
        Vector3? mouseGridPosition = null;
        if (_snapPreviewToCenter)
        {
            if (!TryConvertMouseToGridCenter(out mouseGridPosition))
            {
                return;
            }
        }
        else
        {
            if (!TryConvertMouseToGrid(out mouseGridPosition))
            {
                return;
            }
        }
        _targetTransform.position = mouseGridPosition.Value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_gridBounds.center, _gridBounds.size);
    }
}
