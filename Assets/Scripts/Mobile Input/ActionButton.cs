using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{
    private bool _pressed;
    public bool Pressed
    {
        get => _pressed;
        set { _pressed = false; }
    }

    private bool _held;
    public bool Held
    {
        get => _held;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _held = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _held = false;
    }
}
