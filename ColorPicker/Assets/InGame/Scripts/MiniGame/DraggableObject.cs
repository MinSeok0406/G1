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
            // RectTransform�� �߽� ��ǥ (world space)
            Vector2 objectWorldPos = rectTransform.localPosition;

            // �巡�� ������ ��� (Ŭ�� ��ġ�� ������Ʈ �߽� �� �Ÿ�)
            dragOffset = objectWorldPos - pointerWorldPos;

        }

        public void OnDrag(Vector2 pointerPosition)
        {
            Vector2 targetLocalPos = pointerPosition + dragOffset;

            rectTransform.localPosition = targetLocalPos;
        }

        public void OnEndDrag()
        {
        }
    }
}
