using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 카드 오브젝트
    /// 좌우로만 드래그 가능하며, 일정 거리 이상 드래그하면 성공
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(DraggableObject))]
    public class CardObject : MonoBehaviour
    {
        [SerializeField] private float swipeDistance = 200f; // 스와이프 성공 거리
        [SerializeField] private bool allowLeftSwipe = true; // 왼쪽 스와이프 허용
        [SerializeField] private bool allowRightSwipe = true; // 오른쪽 스와이프 허용

        public Action OnSwipeSuccess;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private bool isSwipeComplete = false;

        private DraggableObject draggable;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            draggable = GetComponent<DraggableObject>();
            startPosition = rectTransform.localPosition;
        }

        private void Update()
        {
            // Y축 고정 (좌우로만 드래그 가능)
            Vector3 currentPos = rectTransform.localPosition;
            rectTransform.localPosition = new Vector3(currentPos.x, startPosition.y, currentPos.z);

            if (isSwipeComplete) return;

            // 카드가 충분히 좌우로 이동했는지 확인
            float distance = rectTransform.localPosition.x - startPosition.x;

            // 오른쪽 스와이프 체크
            if (allowRightSwipe && distance >= swipeDistance)
            {
                CompleteSwipe();
            }
            // 왼쪽 스와이프 체크
            else if (allowLeftSwipe && distance <= -swipeDistance)
            {
                CompleteSwipe();
            }
        }

        private void CompleteSwipe()
        {
            isSwipeComplete = true;
            OnSwipeSuccess?.Invoke();
            Debug.Log("[CardObject] Card swiped successfully!");
        }

        /// <summary>
        /// 카드 초기화
        /// </summary>
        public void ResetCard()
        {
            isSwipeComplete = false;
            rectTransform.localPosition = startPosition;
        }

        /// <summary>
        /// 스와이프 거리 설정
        /// </summary>
        public void SetSwipeDistance(float distance)
        {
            swipeDistance = distance;
        }

        /// <summary>
        /// 스와이프 방향 설정
        /// </summary>
        public void SetSwipeDirection(bool left, bool right)
        {
            allowLeftSwipe = left;
            allowRightSwipe = right;
        }
    }
}
