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

        public void OnBeginDrag(Vector2 pointerWorldPos)
        {
            // RectTransform의 중심 좌표 (world space)
            Vector2 objectWorldPos = rectTransform.localPosition;

            // 드래그 오프셋 계산 (클릭 위치와 오브젝트 중심 간 거리)
            dragOffset = objectWorldPos - pointerWorldPos;

            Debug.Log($"[{name}] 드래그 시작 - dragOffset: {dragOffset}");
        }

        public void OnDrag(Vector2 pointerPosition)
        {
            Vector2 targetLocalPos = pointerPosition + dragOffset;

            rectTransform.localPosition = targetLocalPos;
        }

        public void OnEndDrag()
        {
            Debug.Log($"[{name}] 드래그 끝");
        }
    }
}
