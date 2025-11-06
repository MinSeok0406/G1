using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 주사기 오브젝트
    /// 환자에게 드래그하여 주입
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(DraggableObject))]
    public class SyringeObject : MonoBehaviour
    {
        public Action OnInjectionComplete;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private Vector3 targetPosition;
        private float injectionDistance = 50f;
        private bool isInjected = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startPosition = rectTransform.localPosition;
        }

        private void Update()
        {
            if (isInjected) return;

            // 목표 지점과의 거리 체크
            float distance = Vector3.Distance(rectTransform.position, targetPosition);

            if (distance <= injectionDistance)
            {
                CompleteInjection();
            }
        }

        private void CompleteInjection()
        {
            isInjected = true;
            OnInjectionComplete?.Invoke();
            Debug.Log("[SyringeObject] Injection completed!");
        }

        public void ResetSyringe()
        {
            isInjected = false;
            rectTransform.localPosition = startPosition;
        }

        public void SetInjectionDistance(float distance)
        {
            injectionDistance = distance;
        }

        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = target;
        }
    }
}
