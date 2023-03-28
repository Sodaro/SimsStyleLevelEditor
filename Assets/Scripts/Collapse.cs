using UnityEngine;

public class Collapse : MonoBehaviour
{
    private float _width = 0;
    private float _height = 0;
    private float _x = 0;
    private float _y = 0;
    private RectTransform _rectTransform = null;
    private bool _collapsed = false;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        var rect = _rectTransform.rect;
        _width = rect.width;
        _height = rect.height;
        _x = rect.x;
        _y = rect.y;
    }

    public void ToggleCollapse()
    {
        _collapsed = !_collapsed;

        if (_collapsed)
        {
            _rectTransform.sizeDelta = new Vector2(0, 0);
        }
        else
        {
            _rectTransform.sizeDelta = new Vector2(_width, _height);
        }
    }
}
