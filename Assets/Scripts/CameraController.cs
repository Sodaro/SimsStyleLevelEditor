using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _zoomSpeed = 10f;
    private Vector3 _velocity = Vector3.zero;

    private Transform _cameraTransform;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void UpdateVelocity()
    {
        var moveInput = InputHandler.Instance.PlayerGameplayActions.CameraMove.ReadValue<Vector2>();
        _velocity = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x) * _moveSpeed;
    }

    private void UpdateZoom()
    {
        var zoomInput = InputHandler.Instance.PlayerGameplayActions.CameraZoom.ReadValue<float>();
        float zoom = 0;
        if (zoomInput > 0 )
        {
            zoom = 1;
        }
        else if (zoomInput < 0)
        {
            zoom = -1;
        }
        _cameraTransform.localPosition += _cameraTransform.forward * _zoomSpeed * zoom * Time.deltaTime;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateVelocity();
        UpdateZoom();
        transform.localPosition += _velocity * Time.deltaTime;
    }
}
