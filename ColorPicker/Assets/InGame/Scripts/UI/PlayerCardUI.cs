using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class PlayerCardUI : MonoBehaviour
    {

        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Button playerDeductionButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject deathIcon;

        [Header("Customization Display")]
        [SerializeField] private Image[] customizationImages; // 커스터마이즈 색상을 적용할 이미지 배열

        private int actorNum;
        private string nickname;
        private ColorType deductionColor;
        private bool isAlive;

        public void Initialize(int actorNum, string nickname, bool isAlive)
        {
            this.isAlive = isAlive;
            this.actorNum = actorNum;

            if (playerNameText)
            {
                playerNameText.text = nickname;
                this.nickname = nickname;
            }

            SetAlive(isAlive);

            // 커스터마이즈 색상 자동 적용
            ApplyCustomizationColor();
        }

        private void SetAlive(bool isAlive)
        {
            if (deathIcon) deathIcon.SetActive(!isAlive);
        }

        public void SetDeductionColor(ColorType color = ColorType.White)
        {
            deductionColor = color;
            backgroundImage.color = HelperUtilities.ToUnityColor(color);
        }

        public int GetActorNum() { return actorNum; }
        public int GetColor() { return (int)deductionColor; }


        public void OnClick_ShowPicker()
        {
            UIManager.Instance.ShowDeductionPickerUI(this);
        }

        /// <summary>
        /// 플레이어의 커스터마이즈 정보를 전달받은 UI 요소에 적용
        /// </summary>
        /// <param name="images">색상을 적용할 UI 이미지 배열</param>
        /// <param name="nameText">이름을 표시할 TMP_Text</param>
        public void ApplyCustomizationToUI(Image[] images, TMP_Text nameText)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogWarning("[PlayerCardUI] UIManager instance not found.");
                return;
            }

            UIManager.Instance.ApplyPlayerCustomizationToUI(actorNum, images, nameText);
        }

        /// <summary>
        /// 버튼 클릭 시 커스터마이즈 정보를 표시 (Inspector에서 연결 가능)
        /// </summary>
        public void OnClick_ShowCustomization(Image[] images, TMP_Text nameText)
        {
            ApplyCustomizationToUI(images, nameText);
        }

        /// <summary>
        /// 플레이어 카드의 커스터마이즈 이미지 배열에 색상 적용
        /// </summary>
        private void ApplyCustomizationColor()
        {
            if (customizationImages == null || customizationImages.Length == 0)
            {
                return; // 커스터마이즈 이미지가 설정되지 않은 경우
            }

            if (CustomizeManager.Instance == null)
            {
                Debug.LogWarning("[PlayerCardUI] CustomizeManager instance not found.");
                return;
            }

            int colorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNum);
            if (colorIndex < 0)
            {
                Debug.LogWarning($"[PlayerCardUI] No color assigned for actor {actorNum}");
                return;
            }

            Color color = CustomizeManager.Instance.GetColor(colorIndex);

            int appliedCount = 0;
            foreach (var image in customizationImages)
            {
                if (image != null)
                {
                    image.color = color;
                    appliedCount++;
                }
            }

            Debug.Log($"[PlayerCardUI] Applied color index {colorIndex} to {appliedCount} image(s) for actor {actorNum}");
        }
    }
}
