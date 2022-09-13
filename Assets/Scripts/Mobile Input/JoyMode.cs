using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyMode : Joystick
{
    [SerializeField] private float _moveThreshold = 1f;
    [SerializeField] private JoystickType _joystickType = JoystickType.Fixed;
    public float MoveThreshold
    {
        get => _moveThreshold;
        set { _moveThreshold = Mathf.Abs(value); }
    }
    private Vector2 _fixedPosition = Vector2.zero;

    public void SetMode(JoystickType joystickType)
    {
        _joystickType = joystickType;
        if (_joystickType == JoystickType.Fixed)
        {
            _background.anchoredPosition = _fixedPosition;
            _background.gameObject.SetActive(true);
        }
        else
        {
            _background.gameObject.SetActive(false);
        }
    }
    protected override void Start()
    {
        base.Start();
        _fixedPosition = _background.anchoredPosition;
        SetMode(_joystickType);
    }
    private void Update()
    {
        if (_joystickType == JoystickType.Fixed && !_background.gameObject.activeSelf)
        {
            SetMode(JoystickType.Fixed);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (_joystickType != JoystickType.Fixed)
        {
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            _background.transform.position = eventData.position;
            _background.gameObject.SetActive(true);
        }
        base.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (_joystickType != JoystickType.Fixed)
        {
            _background.gameObject.SetActive(false);
        }
    }
    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (_joystickType == JoystickType.Dynamic && magnitude > MoveThreshold)
        {
            Vector2 difference = normalised * (magnitude - MoveThreshold)*radius;
            _background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}
public enum JoystickType
{
    Fixed,
    Floating,
    Dynamic
}