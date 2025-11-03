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
    }
}
