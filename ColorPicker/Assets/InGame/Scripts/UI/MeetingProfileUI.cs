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

                // 마피아 플레이어면 빨간색으로 표시
                UpdateNameColor(actorNum);
            }

            this.actorNum = actorNum;

            SetAlive(isAlive);

            // 투표 영역 초기화
            if (voteArea != null)
            {
                voteArea.Clear();
            }
        }

        /// <summary>
        /// 로컬 플레이어가 마피아면 다른 마피아 플레이어들을 빨간색으로 표시
        /// </summary>
        private void UpdateNameColor(int targetActorNum)
        {
            if (playerNameText == null) return;

            // 로컬 플레이어의 직업 확인
            int myActorNum = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorNum < 0)
            {
                playerNameText.color = Color.white;
                return;
            }

            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(myActorNum, out var myPrivateData))
            {
                playerNameText.color = Color.white;
                return;
            }

            // 로컬 플레이어가 마피아가 아니면 모두 흰색
            if (myPrivateData.classType != (int)PlayerClassType.mafia)
            {
                playerNameText.color = Color.white;
                return;
            }

            // 로컬 플레이어가 마피아면, 이 플레이어도 마피아인지 확인
            if (GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorNum, out var targetPrivateData))
            {
                if (targetPrivateData.classType == (int)PlayerClassType.mafia)
                {
                    playerNameText.color = Color.red; // 마피아끼리는 빨간색
                }
                else
                {
                    playerNameText.color = Color.white;
                }
            }
            else
            {
                playerNameText.color = Color.white;
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