using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IClickable
    {
        void OnClick(Vector2 localPosition);
    }

    public interface IDraggable
    {
        void OnBeginDrag(Vector2 pointerPosition);
        void OnDrag(Vector2 pointerPosition);
        void OnEndDrag();
    }
}
