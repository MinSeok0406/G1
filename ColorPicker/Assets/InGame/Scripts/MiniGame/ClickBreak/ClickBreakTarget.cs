using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 클릭해서 깨는 타겟 오브젝트
    /// 특정 횟수만큼 클릭하면 파괴됨
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ClickableObject))]
    public class ClickBreakTarget : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int clicksRequired = 5; // 파괴에 필요한 클릭 횟수
        [SerializeField] private float scaleReduction = 0.1f; // 클릭당 크기 감소량

        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color damageColor = Color.red;

        public Action OnTargetBroken;

        private Image image;
        private ClickableObject clickable;
        private RectTransform rectTransform;
        private int currentClicks = 0;
        private Vector3 originalScale;

        private void Awake()
        {
            image = GetComponent<Image>();
            clickable = GetComponent<ClickableObject>();
            rectTransform = GetComponent<RectTransform>();

            originalScale = rectTransform.localScale;

            clickable.OnClickEvent += OnClicked;
            image.color = normalColor;
        }

        private void OnClicked(Vector2 localPosition)
        {
            currentClicks++;

            // 시각적 피드백
            PlayDamageFeedback();

            // 크기 감소
            float scaleMultiplier = 1f - (scaleReduction * currentClicks);
            rectTransform.localScale = originalScale * Mathf.Max(scaleMultiplier, 0.1f);

            Debug.Log($"[ClickBreakTarget] Clicked {currentClicks}/{clicksRequired}");

            if (currentClicks >= clicksRequired)
            {
                BreakTarget();
            }
        }

        private void PlayDamageFeedback()
        {
            StartCoroutine(DamageFeedbackCoroutine());
        }

        private System.Collections.IEnumerator DamageFeedbackCoroutine()
        {
            image.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            image.color = normalColor;
        }

        private void BreakTarget()
        {
            OnTargetBroken?.Invoke();
            gameObject.SetActive(false);
            Debug.Log("[ClickBreakTarget] Target broken!");
        }

        /// <summary>
        /// 타겟 리셋
        /// </summary>
        public void ResetTarget()
        {
            currentClicks = 0;
            rectTransform.localScale = originalScale;
            image.color = normalColor;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 필요한 클릭 횟수 설정
        /// </summary>
        public void SetClicksRequired(int clicks)
        {
            clicksRequired = clicks;
        }

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnClicked;
            }
        }
    }
}
