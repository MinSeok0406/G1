using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region RequireComponent
[RequireComponent(typeof(MouseInteractiveEvent))]
#endregion
[DisallowMultipleComponent]
public class ToggleButtonInteractiveEffect : MonoBehaviour
{
    private MouseInteractiveEvent mouseInteractiveEvent;

    private int toggleIndex = 0;

    [SerializeField] private Image spriteImage;
    [SerializeField] private List<ToggleImage> toggleImages
        = new List<ToggleImage>();


    private void Awake()
    {
        mouseInteractiveEvent = GetComponent<MouseInteractiveEvent>();

        toggleImages[toggleIndex].sprite = spriteImage.sprite;
        SetImageColor(toggleImages[toggleIndex].color);
    }

    private void OnEnable()
    {
        mouseInteractiveEvent.OnPointerDown += MouseInteractiveEvent_OnPointerDown;
    }

    private void OnDisable()
    {
        mouseInteractiveEvent.OnPointerDown -= MouseInteractiveEvent_OnPointerDown;
    }


    private void MouseInteractiveEvent_OnPointerDown()
    {
        ToggleSwitch();

        SetToggleImage(toggleIndex);
    }

    private void ToggleSwitch()
    {
        toggleIndex++;

        if (toggleIndex == toggleImages.Count)
        {
            toggleIndex = 0;
        }
    }

    private void SetToggleImage(int toggleIndex)
    {
        SetImageColor(toggleImages[toggleIndex].color);
        SetImageSprite(toggleImages[toggleIndex].sprite);
    }

    private void SetImageColor(Color color)
    {
        Image[] images = GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            image.color = color;
        }
    }

    private void SetImageSprite(Sprite sprite)
    {
        spriteImage.sprite = sprite;
    }

    #region Validate
#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateCheck.ValidateCheckNullValue(this, nameof(spriteImage), spriteImage);
        ValidateCheck.ValidateCheckEnumerableValues(this, nameof(toggleImages), toggleImages);
    }
#endif
    #endregion 
}

[System.Serializable]
public class ToggleImage
{
    public Sprite sprite;
    public Color color = new Color(1f, 1f, 1f, 1f);
}

