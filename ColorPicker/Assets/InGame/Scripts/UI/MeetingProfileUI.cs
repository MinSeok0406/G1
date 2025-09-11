using ColorPicker.InGame;
using TMPro;
using UnityEngine;

namespace ColorPicker.inGame
{
    public class MeetingProfileUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private GameObject deathIcon;

        private int actorNum;
        private string nickname;

        public void Initialize(int actorNum, string nickname, bool isAlive)
        {
            if (playerNameText)
            {
                playerNameText.richText = false;

                playerNameText.text = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname.Trim();
            }

            this.actorNum = actorNum;

            SetAlive(isAlive);
        }

        public void Initialzed(int actorNum, string nickname, bool isAlive) => Initialize(actorNum,nickname, isAlive);

        private void SetAlive(bool isAlive)
        {
            if (deathIcon) deathIcon.gameObject.SetActive(!isAlive);
        }

        public void OnClick_ShowVotePopup()
        {
            UIManager.Instance.ShowVotePopup(actorNum, playerNameText.text);
        }
    }
}