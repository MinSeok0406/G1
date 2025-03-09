using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [DisallowMultipleComponent]
    public class ColorChangedEvent : MonoBehaviour
    {
        public event Action<ColorChangedEvent, ColorChangedEventArgs> OnColorChagned;

        public void CallColorChagedEvent(ColorType color)
        {
            OnColorChagned?.Invoke(this, new ColorChangedEventArgs()
            {
                color = color
            });
        }
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public ColorType color;
    }
}
