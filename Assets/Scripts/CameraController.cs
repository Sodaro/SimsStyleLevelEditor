using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

[RequireComponent(typeof(PlayerInput))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _zoomSpeed = 10f;

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _zoomAction;
    private Vector3 _velocity = Vector3.zero;

    private Transform _cameraTransform;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {

        _zoomAction = _playerInput.actions["zoom"];
        _moveAction = _playerInput.actions["move"];

        _moveAction.Enable();
        _zoomAction.Enable();
    }
    private void OnDisable()
    {
        _moveAction.Disable();
        _zoomAction.Disable();
    }


    void UpdateVelocity()
    {
        var moveDir = _moveAction.ReadValue<Vector2>();
        _velocity = Vector3.Normalize(transform.forward * moveDir.y + transform.right * moveDir.x) * _moveSpeed;
    }

    void UpdateZoom()
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
    void Update()
    {
        UpdateVelocity();
        UpdateZoom();
        transform.localPosition += _velocity * Time.deltaTime;
    }
}
