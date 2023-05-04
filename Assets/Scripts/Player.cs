using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlacementGrid _placementGrid;
    [SerializeField] private InteractionType _activeInteractionType;
    enum InteractionType { Build, Selection, Interaction, Demolish };
    public enum MouseClickType { Pressed, Released, Held, None };

    private Camera _camera = null;
    private bool _clickIsHeld = false;
    private GameObject _selectedObject = null;

    private MouseClickType _clickState = MouseClickType.None;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void UpdateMouseState()
    {
        var click = InputHandler.Instance.PlayerGameplayActions.Click;
        bool pressed = click.WasPressedThisFrame();
        bool released = click.WasReleasedThisFrame();
        MouseClickType clickType = MouseClickType.None;
        if (pressed && !InputHandler.Instance.IsHoveringUI)
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
        _clickState = clickType;

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

    private void UpdateInteractionMode()
    {
        var actions = InputHandler.Instance.PlayerGameplayActions;
        var buildInput = actions.Build.WasPressedThisFrame();
        var selectInput = actions.Select.WasPressedThisFrame();
        var interactInput = actions.Interact.WasPressedThisFrame();
        var demolishInput = actions.Demolish.WasPressedThisFrame();
        if (buildInput)
            _activeInteractionType = InteractionType.Build;
        else if (selectInput)
            _activeInteractionType = InteractionType.Selection;
        else if (interactInput)
            _activeInteractionType = InteractionType.Interaction;
        else if (demolishInput)
            _activeInteractionType = InteractionType.Demolish;
    }

    private void UpdateHeight()
    {
        if (!InputHandler.Instance.PlayerGameplayActions.HeightChange.WasPressedThisFrame())
        {
            return;
        }
        float heightChange = InputHandler.Instance.PlayerGameplayActions.HeightChange.ReadValue<float>();
        if (heightChange > 0)
        {
            _placementGrid.IncreasePlaneHeight();
        }
        else if (heightChange < 0)
        {
            _placementGrid.DecreasePlaneHeight();
        }
    }

    private void HandleSelectMode()
    {
        if (_selectedObject == null)
        {
            if (_clickState == MouseClickType.Pressed)
            {
                AttemptSelect();
            }
        }
        else
        {
            if (InputHandler.Instance.PlayerGameplayActions.Delete.WasPressedThisFrame())
            {
                DestroySelectedObject();
            }
            else
            {
                AttemptMoveObject();
                if (_clickState == MouseClickType.Released)
                {
                    _selectedObject = null;
                }
            }
        }
    }

    private void HandleBuildMode()
    {
        _placementGrid.HandleMouseClick(_clickState);
    }

    private void HandleInteractMode()
    {

    }
    private void HandleDemolishMode()
    {
        if (_clickState == MouseClickType.Released)
        {
            AttemptDemolish();
        }
    }

    private void Update()
    {
        UpdateHeight();
        UpdateMouseState();
        UpdateInteractionMode();

        switch (_activeInteractionType)
        {
            case InteractionType.Build:
                HandleBuildMode();
                break;
            case InteractionType.Selection:
                HandleSelectMode();
                break;
            case InteractionType.Interaction:
                HandleInteractMode();
                break;
            case InteractionType.Demolish:
                HandleDemolishMode();
                break;
        }
    }
    private GameObject GetObjectAtMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1))
        {
            return hit.transform.gameObject;
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
    private void DestroySelectedObject()
    {
        if (_selectedObject != null)
        {
            Destroy(_selectedObject);
            _selectedObject = null;
        }
    }
    private void AttemptDemolish()
    {
        var obj = GetObjectAtMouse();
        if (obj != null)
        {
            Destroy(obj.transform.gameObject);
        }
    }
}