using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(RectTransform))]
    public class ClickableObject : MonoBehaviour, IClickable
    {
        public Action OnClickEvent;

        public void OnClick()
        {
            Debug.Log("Click");
            OnClickEvent?.Invoke();
        }
    }
}
