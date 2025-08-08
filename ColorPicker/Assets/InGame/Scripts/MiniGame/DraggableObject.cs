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
        private Vector2 halfSize;
        private float minX, maxX, minY, maxY;

        private RectTransform dragArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            Initialized();
        }

        private void Initialized()
        {
            dragArea = MiniGameManager.Instance.dragArea;

            halfSize = new Vector2(
                    rectTransform.rect.width * rectTransform.lossyScale.x / 2f,
                    rectTransform.rect.height * rectTransform.lossyScale.y / 2f
            );

            minX = dragArea.rect.xMin + halfSize.x;
            maxX = dragArea.rect.xMax - halfSize.x;
            minY = dragArea.rect.yMin + halfSize.y;
            maxY = dragArea.rect.yMax - halfSize.y;

        }

        public void OnBeginDrag()
        {
            Vector2 pointerLocalPos = HelperUtilities.ScreenToLocalPointInRect(canvas, canvas.transform as RectTransform,
                MiniGameInputHandler.GetPosition());

            dragOffset = (Vector2)rectTransform.localPosition - pointerLocalPos;

            Debug.Log($"[{name}] 드래그 시작");
        }

        public void OnDrag(Vector2 pointerPosition)
        {
            Vector2 pointerLocalPos = HelperUtilities.ScreenToLocalPointInRect(canvas, canvas.transform as RectTransform, pointerPosition);
            Vector2 targetLocalPos = pointerLocalPos + dragOffset;

            if (dragArea != null)
            {
                Vector2 areaSize = dragArea.rect.size;
                
                targetLocalPos.x = Mathf.Clamp(targetLocalPos.x, minX, maxX);
                targetLocalPos.y = Mathf.Clamp(targetLocalPos.y, minY, maxY);
            }

            rectTransform.localPosition = targetLocalPos;
        }

        public void OnEndDrag()
        {
            Debug.Log($"[{name}] 드래그 종료");
        }
    }
}
