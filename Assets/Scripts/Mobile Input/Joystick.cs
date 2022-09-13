using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//This script should be attached to the Background image of the joystick
public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region SerializeField
    [SerializeField] private float _handleRange = 1;
    [SerializeField] private float _deadZone = 0;
    [SerializeField] private AxisOptions _axisOption = AxisOptions.Both;
    [SerializeField] private bool _snapX = false;
    [SerializeField] private bool _snapY = false;
    [SerializeField] protected RectTransform _background = null;
    [SerializeField] private RectTransform _handle = null;
    #endregion
    #region References
    private RectTransform _baseRect = null;
    private Canvas _canvas;
    [SerializeField] protected Camera _cam;
    public Vector2 input = Vector2.zero;
    #endregion
    #region Properties
    public bool SnapX
    {
        get { return _snapX; }
        set { _snapX = value; }
    }
    public bool SnapY
    {
        get { return _snapY; }
        set { _snapY = value; }
    }
    public float HandleRange
    {
        get { return _handleRange; }
        set { _handleRange = Mathf.Abs(value); }
    }
    public float DeadZone
    {
        get { return _deadZone; }
        set { _deadZone = Mathf.Abs(value); }
    }
    public AxisOptions AxisOption
    {
        get { return _axisOption; }
        set { _axisOption = value; }
    }
    public float Horizontal
    {
        get { return SnapX ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x; }
    }
    public float Vertical
    {
        get { return SnapY ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; }
    }
    public Vector2 Direction
    {
        get { return new Vector2(Horizontal, Vertical); }
    }
    #endregion
    #region Interface
    protected virtual void Start()
    {
        //not sure why these are here atm - 3pm 24/08
        HandleRange = _handleRange;
        DeadZone = _deadZone;

        //fetch the rect transform from the Background
        _baseRect = GetComponent<RectTransform>();
        //fetch the canvas
        _canvas = GetComponentInParent<Canvas>();

        if (_canvas == null)
        {
            Debug.LogError("The Joystick background is not placed directly inside the canvas, or this script is not placed on the background image");
        }
        Vector2 centre = new Vector2(0.5f, 0.5f);
        _background.pivot = centre;
        _handle.anchorMin = centre;
        _handle.anchorMax = centre;
        _handle.pivot = centre;
        _handle.anchoredPosition = Vector2.zero;
    }
    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0)
        {
            return value;
        }
        if (AxisOption == AxisOptions.Both)
        {
            float angle = Vector2.Angle(input, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                {
                    return 0;
                }
                else
                {
                    return value > 0 ? 1 : -1;
                }
            }
            if (snapAxis == AxisOptions.Vertical)
            {
                if (angle < 67.5f || angle > 112.5)
                {
                    return 0;
                }
                else
                {
                    return value > 0 ? 1 : -1;
                }
            }
            return 0;
        }
        else
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
        }

        return 0;
    }
    private void FormatInput()
    {
        if (AxisOption == AxisOptions.Horizontal)
        {
            input = new Vector2(input.x, 0f);
        }
        else if (AxisOption == AxisOptions.Vertical)
        {
            input = new Vector2(0f, input.y);
        }
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > DeadZone)
        {
            if (magnitude > 1)
            {
                input = normalised;
            }
        }
        else
        {
            input = Vector2.zero;
        }
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint = Vector2.zero;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
        {
            return localPoint - (_background.anchorMax * _baseRect.sizeDelta);
        }
        return Vector2.zero;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (_cam == null)
        {
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _cam = _canvas.worldCamera;
            }
        }
        Vector2 position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
        Vector2 radius = _background.sizeDelta / 2f;
        input = (eventData.position - position) / (radius * _canvas.scaleFactor);
        FormatInput();
        HandleInput(input.magnitude, input.normalized, radius, _cam);
        _handle.anchoredPosition = input * radius * HandleRange;
        //if this plays up we will be changing HandleRange to _handleRange;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        _handle.anchoredPosition = Vector2.zero;
    }
    #endregion
}

public enum AxisOptions
{
    Both, Horizontal, Vertical
}