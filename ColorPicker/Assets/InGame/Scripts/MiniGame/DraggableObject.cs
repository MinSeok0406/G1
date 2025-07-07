using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableObject : MonoBehaviour, IDraggable
    {
        [SerializeField] private Canvas canvas;
        private RectTransform rectTransform;
        private Vector2 dragOffset;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                MiniGameInputHandler.GetPosition(),
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 pointerLocalPos
            );

            dragOffset = (Vector2)rectTransform.localPosition - pointerLocalPos;

            Debug.Log($"[{name}] 드래그 시작");
        }

        public void OnDrag(Vector2 pointerPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                pointerPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 pointerLocalPos
            );

            rectTransform.localPosition = pointerLocalPos + dragOffset;
        }

        public void OnEndDrag()
        {
            Debug.Log($"[{name}] 드래그 끝");
        }
    }
}
