using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region RequireComponent
[RequireComponent(typeof(MouseInteractiveEvent))]
#endregion
[DisallowMultipleComponent]
public class ButtonInteractiveEffect : MonoBehaviour
{
    private MouseInteractiveEvent mouseInteractiveEvent;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 255f);
    [SerializeField] private Color pressColor = new Color(0.5f, 0.5f, 0.5f, 255f); 

    private void Awake()
    {
        mouseInteractiveEvent = GetComponent<MouseInteractiveEvent>();

        SetImageColor(normalColor);
    }

    private void OnEnable()
    {
        mouseInteractiveEvent.OnPointerDown += MouseInteractiveEvent_OnPointerDown;
        mouseInteractiveEvent.OnPointerUp += MouseInteractiveEvent_OnPointerUp;
    }

    private void OnDisable()
    {
        mouseInteractiveEvent.OnPointerDown -= MouseInteractiveEvent_OnPointerDown;
        mouseInteractiveEvent.OnPointerUp -= MouseInteractiveEvent_OnPointerUp;
    }

    private void MouseInteractiveEvent_OnPointerUp()
    {
        SetImageColor(normalColor);
    }

    private void MouseInteractiveEvent_OnPointerDown()
    {
        SetImageColor(pressColor);
    }

    private void SetImageColor(Color color)
    {
        Image[] images = GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            image.color = color;
        }
    }
}
