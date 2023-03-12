using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    enum InteractionType { Build, Selection, Interaction, Demolish };
    public enum MouseClickType { Pressed, Released, Held, None };

    [SerializeField] private InputAction _clickAction;
    [SerializeField] private InputAction _buildAction;
    [SerializeField] private InputAction _selectAction;
    [SerializeField] private InputAction _interactionAction;
    [SerializeField] private InputAction _demolishAction;

    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private InteractionType _activeInteractionType;

    private Camera _camera = null;
    private bool _clickIsHeld = false;
    private GameObject _selectedObject = null;

    private void Awake()
    {
        _clickAction.Enable();
        _buildAction.Enable();
        _selectAction.Enable();
        _interactionAction.Enable();
        _demolishAction.Enable();

        _camera = Camera.main;
    }

    private void GetInput()
    {

    }

    private void Update()
    {
        bool pressed = _clickAction.WasPressedThisFrame();
        bool released = _clickAction.WasReleasedThisFrame();
        MouseClickType clickType = MouseClickType.None;
        if (pressed)
        {
            clickType = MouseClickType.Pressed;
        }
        else if (released)
        {
            clickType = MouseClickType.Released;
        }
        else if (_clickIsHeld)
        {
            clickType = MouseClickType.Held;
        }

        var buildInput = _buildAction.WasPressedThisFrame();
        var selectInput = _selectAction.WasPressedThisFrame();
        var interactInput = _interactionAction.WasPressedThisFrame();
        var demolishInput = _demolishAction.WasPressedThisFrame();

        if (buildInput)
            _activeInteractionType = InteractionType.Build;
        else if (selectInput)
            _activeInteractionType = InteractionType.Selection;
        else if (interactInput)
            _activeInteractionType = InteractionType.Interaction;
        else if (demolishInput)
            _activeInteractionType = InteractionType.Demolish;

        switch (_activeInteractionType)
        {
            case InteractionType.Build:
                _placementGrid.HandleMouseClick(clickType);
                break;
            case InteractionType.Selection:
                if (clickType == MouseClickType.Released)
                {
                    AttemptSelect();
                }
                break;
            case InteractionType.Interaction:
                AttemptMoveObject();
                break;
            case InteractionType.Demolish:
                if (clickType == MouseClickType.Released)
                {
                    AttemptDemolish();
                }
                break;
        }

        //update hold logic at end of frame
        if (pressed)
        {
            _clickIsHeld = true;
        }
        else if (released)
        {
            _clickIsHeld = false;
        }
    }
    GameObject GetObjectAtMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    void AttemptSelect()
    {
        _selectedObject = GetObjectAtMouse();
    }

    void AttemptMoveObject()
    {
        if (_selectedObject == null)
            return;

        _placementGrid.SnapObjectToGrid(_selectedObject);
    }
    void AttemptDemolish()
    {
        var obj = GetObjectAtMouse();
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}