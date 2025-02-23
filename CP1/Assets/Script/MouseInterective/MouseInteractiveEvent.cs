using System;
using UnityEngine;

public class MouseInteractiveEvent : MonoBehaviour
{
    public event Action OnPointerDown;
    public event Action OnPointerUp;

    public void CallPointerDownEvent()
    {
        OnPointerDown?.Invoke();
    }

    public void CallPointerUpEvent()
    {
        OnPointerUp?.Invoke();
    }
}
