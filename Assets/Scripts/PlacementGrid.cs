using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementGrid : MonoBehaviour
{
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] int _gridSize = 100;
    Material lineMaterial;

    private (Vector3, Vector3)[] _gridVertices;

    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _snapToCenter = false;

    private PlayerInput _playerInput;
    private InputAction _clickAction;

    private Vector3? _buildStartPoint = Vector3.zero;
    private Vector3? _buildEndPoint = Vector3.zero;

    Plane plane;

    private Bounds _gridBounds;

    public bool IsInsideGridBounds(Vector3 worldPosition)
    {
        return false;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
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

    private void OnEnable()
    {
        _clickAction = _playerInput.actions["click"];
        _clickAction.Enable();
        _clickAction.started += OnClickPressed;
        _clickAction.canceled += OnClickReleased;
    }
    private void OnDisable()
    {
        _clickAction.Disable();
        _clickAction.started -= OnClickPressed;
        _clickAction.canceled -= OnClickReleased;
    }


    private void OnClickPressed(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            if (_snapToCenter)
            {
                var desiredPosition = GridUtilities.GetTileCenterFromWorldXZ(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _buildStartPoint = desiredPosition;
                }
            }
            else
            {
                var desiredPosition = GridUtilities.GetGridPosition(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _buildStartPoint = desiredPosition;
                }
            }
        }
    }

    private void OnClickReleased(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            if (_snapToCenter)
            {
                var desiredPosition = GridUtilities.GetTileCenterFromWorldXZ(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _buildEndPoint = desiredPosition;
                }
            }
            else
            {
                var desiredPosition = GridUtilities.GetGridPosition(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _buildEndPoint = desiredPosition;
                }
            }
        }
        if (_buildEndPoint != null && _buildStartPoint != null)
        {
            var diff = _buildEndPoint - _buildStartPoint;
            var center = (_buildStartPoint + _buildEndPoint) / 2;
            var instance = Instantiate(_wallPrefab, center.Value, Quaternion.identity, transform.parent);
            instance.transform.localScale = new Vector3(Mathf.Abs(diff.Value.x), 1, Mathf.Abs(diff.Value.z));
            _buildEndPoint = null;
            _buildStartPoint = null;
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

    private void Update()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            if (_snapToCenter)
            {
                var desiredPosition = GridUtilities.GetTileCenterFromWorldXZ(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _targetTransform.position = desiredPosition;
                }
            }
            else
            {
                var desiredPosition = GridUtilities.GetGridPosition(worldPos);
                if (_gridBounds.Contains(desiredPosition))
                {
                    _targetTransform.position = desiredPosition;
                }
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_gridBounds.center, _gridBounds.size);
    }
}
