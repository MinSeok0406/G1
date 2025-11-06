using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 순서대로 클릭해야 하는 타겟
    /// 순서가 맞을 때만 클릭 가능
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ClickableObject))]
    public class SequenceClickTarget : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 비활성 (숨김)
        [SerializeField] private Color activeColor = Color.white; // 활성 (표시됨)
        [SerializeField] private Color clickedColor = Color.green; // 클릭됨

        [Header("UI")]
        [SerializeField] private TMPro.TMP_Text numberText; // 순서 번호 표시 (선택 사항)

        public Action<int> OnTargetClicked; // 순서 번호를 전달

        private Image image;
        private ClickableObject clickable;
        private int sequenceNumber;
        private bool isActive = false;
        private bool isClicked = false;

        private void Awake()
        {
            image = GetComponent<Image>();
            clickable = GetComponent<ClickableObject>();

            if (numberText == null)
            {
                numberText = GetComponentInChildren<TMPro.TMP_Text>();
            }

            clickable.OnClickEvent += OnClicked;

            SetInactive();
        }

        private void OnClicked(Vector2 localPosition)
        {
            if (!isActive || isClicked) return;

            isClicked = true;
            image.color = clickedColor;

            OnTargetClicked?.Invoke(sequenceNumber);

            Debug.Log($"[SequenceClickTarget] Target {sequenceNumber} clicked!");
        }

        /// <summary>
        /// 타겟 활성화 (표시)
        /// </summary>
        public void Activate(int number)
        {
            sequenceNumber = number;
            isActive = true;
            isClicked = false;

            image.color = activeColor;

            if (numberText != null)
            {
                numberText.text = number.ToString();
                numberText.gameObject.SetActive(true);
            }

            gameObject.SetActive(true);

            Debug.Log($"[SequenceClickTarget] Target {number} activated!");
        }

        /// <summary>
        /// 타겟 비활성화 (숨김)
        /// </summary>
        public void SetInactive()
        {
            isActive = false;
            isClicked = false;
            image.color = inactiveColor;

            if (numberText != null)
            {
                numberText.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 타겟 리셋
        /// </summary>
        public void ResetTarget()
        {
            SetInactive();
        }

        public int GetSequenceNumber() => sequenceNumber;
        public bool IsClicked() => isClicked;

        private void OnDestroy()
        {
            if (clickable != null)
            {
                clickable.OnClickEvent -= OnClicked;
            }
        }
    }
}
