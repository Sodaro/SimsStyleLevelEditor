using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private EventSystem _eventSystem;
    public bool IsHoveringUI => _eventSystem.IsPointerOverGameObject();
    public static InputHandler Instance { get; private set; }
    public PlayerInputActions InputActions {get; private set;}
    public PlayerInputActions.GameplayActions PlayerGameplayActions { get; private set;}
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void OnEnable()
    {
        if (InputActions == null)
        {
            InputActions = new PlayerInputActions();
        }
        PlayerGameplayActions = InputActions.Gameplay;
        PlayerGameplayActions.Enable();
    }

    private void OnDisable()
    {
        if (InputActions != null )
        {
            PlayerGameplayActions.Disable();
        }
    }
}
