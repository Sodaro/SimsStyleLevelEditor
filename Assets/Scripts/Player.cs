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
    [SerializeField] private InputAction _deleteAction;
    [SerializeField] private InputAction _heightIncreaseAction;
    [SerializeField] private InputAction _heightDecreaseAction;

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
        _deleteAction.Enable();
        _heightIncreaseAction.Enable();
        _heightDecreaseAction.Enable();

        _camera = Camera.main;
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
        var heightIncreaseInput = _heightIncreaseAction.WasPressedThisFrame();
        var heightDecreaseInput = _heightDecreaseAction.WasPressedThisFrame();

        if (heightIncreaseInput)
        {
            _placementGrid.IncreasePlaneHeight();
        }
        else if (heightDecreaseInput)
        {
            _placementGrid.DecreasePlaneHeight();
        }

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
                if (_selectedObject == null)
                {
                    if (clickType == MouseClickType.Pressed)
                    {
                        AttemptSelect();
                    }
                }
                else
                {
                    if (_deleteAction.WasPressedThisFrame())
                    {
                        AttemptDemolish();
                    }
                    else
                    {
                        AttemptMoveObject();
                        if (clickType == MouseClickType.Released)
                        {
                            _selectedObject = null;
                        }
                    }
                }
                break;
            case InteractionType.Interaction:
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
    private GameObject GetObjectAtMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1))
        {
            return hit.transform.parent.gameObject;
        }
        return null;
    }

    private void AttemptSelect()
    {
        _selectedObject = GetObjectAtMouse();
    }

    private void AttemptMoveObject()
    {
        if (_selectedObject == null)
            return;

        _placementGrid.SnapObjectToGrid(_selectedObject);
    }
    private void AttemptDemolish()
    {
        var obj = GetObjectAtMouse();
        if (obj != null)
        {
            Destroy(obj.transform.parent.gameObject);
        }
    }
}