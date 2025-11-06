using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 답안 버튼
    /// MentalStateExam 미니게임에 사용되는 버튼 (ClickableObject 사용)
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ClickableObject))]
    public class AnswerButton : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color wrongColor = Color.red;
        [SerializeField] private Color disabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("UI")]
        [SerializeField] private TMPro.TMP_Text answerText; // 답안 텍스트

        public Action<int> OnAnswerClicked; // 답안 값을 전달

        private Image image;
        private ClickableObject clickable;
        private bool isInteractable = true;
        private int answerValue = 0;

        private void Awake()
        {
            image = GetComponent<Image>();
            clickable = GetComponent<ClickableObject>();

            // answerText가 할당되지 않았다면 자식에서 찾기
            if (answerText == null)
            {
                answerText = GetComponentInChildren<TMPro.TMP_Text>();
            }

            clickable.OnClickEvent += OnClicked;
            image.color = normalColor;
        }

        private void OnClicked(Vector2 localPosition)
        {
            if (isInteractable)
            {
                OnAnswerClicked?.Invoke(answerValue);
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
        /// 답안 설정
        /// </summary>
        public void SetAnswer(int value)
        {
            answerValue = value;

            if (answerText != null)
            {
                answerText.text = value.ToString();
            }
        }

        /// <summary>
        /// 답안 값 가져오기
        /// </summary>
        public int GetAnswer()
        {
            return answerValue;
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
            Color originalColor = image.color;
            image.color = feedbackColor;
            yield return new WaitForSeconds(0.3f);
            image.color = originalColor;
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;

            if (!interactable && image != null)
            {
                image.color = disabledColor;
            }
            else if (interactable && image != null)
            {
                image.color = normalColor;
            }
        }

        /// <summary>
        /// 색상 리셋
        /// </summary>
        public void ResetColor()
        {
            if (image != null)
            {
                image.color = normalColor;
            }
        }
    }
}
