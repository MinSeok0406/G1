using TMPro;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MeetingProfileUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private GameObject deathIcon;
        [SerializeField] private VoteArea voteArea;

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

            // 투표 영역 초기화
            if (voteArea != null)
            {
                voteArea.Clear();
            }
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

        /// <summary>
        /// 득표수 업데이트
        /// </summary>
        /// <param name="voteCount">받은 투표 수</param>
        public void UpdateVoteCount(int voteCount)
        {
            if (voteArea != null)
            {
                voteArea.SetVoteCount(voteCount);
            }
        }

        /// <summary>
        /// ActorNumber 반환
        /// </summary>
        public int GetActorNum() => actorNum;

        /// <summary>
        /// 투표 영역 초기화
        /// </summary>
        public void ClearVotes()
        {
            if (voteArea != null)
            {
                voteArea.Clear();
            }
        }
    }
}