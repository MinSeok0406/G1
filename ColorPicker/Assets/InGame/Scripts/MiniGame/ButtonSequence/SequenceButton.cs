using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 순서 버튼
    /// ButtonSequence 미니게임에 사용되는 버튼 (ClickableObject 사용)
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ClickableObject))]
    public class SequenceButton : MonoBehaviour
    {
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color wrongColor = Color.red;
        [SerializeField] private Color disabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        public Action OnButtonClicked;

        private Image image;
        private ClickableObject clickable;
        private bool isInteractable = true;

        private void Awake()
        {
            image = GetComponent<Image>();
            clickable = GetComponent<ClickableObject>();

            clickable.OnClickEvent += OnClicked;

            image.color = normalColor;
        }

        private void OnClicked(Vector2 localPosition)
        {
            if (isInteractable)
            {
                OnButtonClicked?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnClicked;
            }
        }

        /// <summary>
        /// 버튼 하이라이트 (순서 표시 시)
        /// </summary>
        public void Highlight()
        {
            image.color = highlightColor;
        }

        /// <summary>
        /// 하이라이트 해제
        /// </summary>
        public void Unhighlight()
        {
            image.color = normalColor;
        }

        /// <summary>
        /// 정답 피드백
        /// </summary>
        public void PlayCorrectFeedback()
        {
            StartCoroutine(ColorFeedbackCoroutine(correctColor));
        }

        /// <summary>
        /// 오답 피드백
        /// </summary>
        public void PlayWrongFeedback()
        {
            StartCoroutine(ColorFeedbackCoroutine(wrongColor));
        }

        private System.Collections.IEnumerator ColorFeedbackCoroutine(Color feedbackColor)
        {
            image.color = feedbackColor;
            yield return new WaitForSeconds(0.3f);
            image.color = normalColor;
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;

            // 비활성화 시 시각적 피드백
            if (!interactable && image != null)
            {
                image.color = disabledColor;
            }
            else if (interactable && image != null)
            {
                image.color = normalColor;
            }
        }
    }
}
