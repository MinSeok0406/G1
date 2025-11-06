using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClickableObject : MonoBehaviour, IClickable
    {
        public Action<Vector2> OnClickEvent;

        private void Awake()
        {
            SetupLayer();
            SetupCollider();
        }

        private void SetupLayer()
        {
            // Layer를 CRT로 설정 (MiniGameInputHandler가 감지하도록)
            int crtLayer = LayerMask.NameToLayer("CRT");

            if (crtLayer == -1)
            {
                Debug.LogError("[ClickableObject] 'CRT' Layer가 존재하지 않습니다! " +
                    "Edit -> Project Settings -> Tags and Layers에서 'CRT' 레이어를 추가하세요.", this);
            }
            else
            {
                gameObject.layer = crtLayer;
                Debug.Log($"[ClickableObject] Layer 'CRT' 설정 완료: {gameObject.name}", this);
            }
        }

        private void SetupCollider()
        {
            var boxCollider = GetComponent<BoxCollider2D>();
            var rectTransform = GetComponent<RectTransform>();

            if (boxCollider != null && rectTransform != null)
            {
                // RectTransform 크기에 맞춰 Collider 크기 자동 조정
                boxCollider.size = rectTransform.rect.size;
                boxCollider.offset = Vector2.zero;

                Debug.Log($"[ClickableObject] Collider 설정 완료 - Size: {boxCollider.size}, Object: {gameObject.name}", this);
            }
        }

        /// <summary>
        /// 클릭 시 호출 (좌표 매핑된 로컬 좌표 전달)
        /// </summary>
        /// <param name="localPosition">targetRect 기준 로컬 좌표</param>
        public void OnClick(Vector2 localPosition)
        {
            Debug.Log($"[ClickableObject] Clicked! Position: {localPosition}, Object: {gameObject.name}");
            OnClickEvent?.Invoke(localPosition);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 에디터에서 Collider 영역 시각화
            var boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, boxCollider.size);
            }
        }
#endif
    }
}
