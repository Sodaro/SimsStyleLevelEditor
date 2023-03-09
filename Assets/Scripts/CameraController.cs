using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _zoomAction;
    private Vector3 _velocity = Vector3.zero;

    private Transform _cameraTransform;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _moveAction.Enable();
        _zoomAction.Enable();
    }

    private void UpdateVelocity()
    {
        var moveDir = _moveAction.ReadValue<Vector2>();
        _velocity = Vector3.Normalize(transform.forward * moveDir.y + transform.right * moveDir.x) * _moveSpeed;
    }

    private void UpdateZoom()
    {
        var zoomDir = _zoomAction.ReadValue<float>();
        if (zoomDir > 0)
        {
            zoomDir = 1;
        }
        else if (zoomDir < 0)
        {
            zoomDir = -1;
        }
        _cameraTransform.localPosition += _cameraTransform.forward * _zoomSpeed * zoomDir * Time.deltaTime;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateVelocity();
        UpdateZoom();
        transform.localPosition += _velocity * Time.deltaTime;
    }
}
