using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IClickable
    {
        void OnClick();
    }

    public interface IDraggable
    {
        void OnBeginDrag();
        void OnDrag(Vector2 pointerPosition);
        void OnEndDrag();
    }
}
