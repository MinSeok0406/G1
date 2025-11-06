using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MeetingState : GameState
    {
        GameManager gameManager;

        public MeetingState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            gameManager ??= GameManager.Instance;

            gameManager.SetMeetingTimer();
            gameManager.BroadcastMeetingTimer();

            // 미팅 프로필 UI 최신화 (죽은 플레이어 상태 반영)
            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                UIManager.Instance?.InitializePlayerProfile();
            }

            UIManager.Instance.ShowMeetingUI(true);
        }
        public override void Exit()
        {
            base.Exit();

            // 투표 결과 처리 (호스트만)
            if (PhotonNetwork.IsMasterClient)
            {
                ProcessVoteResult();
            }

            UIManager.Instance.ShowMeetingUI(false);

            // 채팅창 클리어
            ClearChatUI();

            // 미팅 후 스킬 초기화 (호스트만)
            if (PhotonNetwork.IsMasterClient)
            {
                ResetAbilities();
            }
        }

        /// <summary>
        /// 채팅창 클리어
        /// </summary>
        private void ClearChatUI()
        {
            var chatUI = UnityEngine.Object.FindObjectOfType<ColorPicker.Chat.ChatUIController>();
            if (chatUI != null)
            {
                chatUI.ClearUI();
            }
        }

        /// <summary>
        /// [Host Only] 미팅 후 스킬 초기화
        /// </summary>
        private void ResetAbilities()
        {
            // 모든 플레이어 순회
            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers == null) return;

            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                // 뷰 ID 가져오기
                if (!GameDataManager.Instance.TryGetViewIDByUID(playerData.googleUID, out int viewID))
                    continue;

                // 개인 데이터 가져오기
                if (!GameDataManager.Instance.TryGetPrivatePlayerData(playerData.googleUID, out var privateData))
                    continue;

                // 마피아 스킬 초기화
                if (privateData.classType == (int)PlayerClassType.mafia)
                {
                    AbilityManager.Instance?.ResetMafiaAbility(viewID);
                }
                // 탐정 스킬 초기화
                else if (privateData.classType == (int)PlayerClassType.detective)
                {
                    AbilityManager.Instance?.ResetDetectiveAbility(viewID);
                }
            }
        }

        /// <summary>
        /// [Host Only] 투표 결과를 처리하고 최다 득표자를 처형
        /// </summary>
        private void ProcessVoteResult()
        {
            int mostVotedActor = gameManager.GetMostVotedPlayer();

            if (mostVotedActor <= 0)
            {
                Debug.Log("[MeetingState] No one was voted out (tie or no votes).");
                return;
            }

            // 최다 득표자 사망 처리
            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(mostVotedActor, out var playerData))
            {
                // 사망 상태 설정 (isAlive = false)
                GameDataManager.Instance.RequestSetPlayerAliveState(playerData.googleUID, false);

                // classType을 ghost로 변경 (마피아 킬과 동일하게 처리)
                if (GameDataManager.Instance.TryGetPrivatePlayerData(playerData.googleUID, out var privateData))
                {
                    privateData.classType = (int)PlayerClassType.ghost;
                    GameDataManager.Instance.UpdatePrivatePlayerData(privateData);
                }

                // 해당 플레이어의 DeathEvent 호출 (RPC)
                if (GameDataManager.Instance.TryGetViewIDByUID(playerData.googleUID, out int viewID))
                {
                    PhotonView targetView = PhotonView.Find(viewID);
                    if (targetView != null)
                    {
                        var player = targetView.GetComponent<Player>();
                        if (player != null)
                        {
                            player.TriggerVoteDeath();
                        }
                    }
                }

                Debug.Log($"[MeetingState] Player {playerData.nickname} (Actor {mostVotedActor}) was voted out.");

                // 컷신 재생 (투표 처형)
                CutsceneManager.Instance?.Host_PlayForAll("Vote_Kill");
            }
        }

        public override void Update()
        {
            base.Update();

            if (!PhotonNetwork.IsMasterClient) return;  

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // 언더런 방지
            gameManager.meetingTimer = Mathf.Max(0f, gameManager.meetingTimer - dt);

            if (gameManager.meetingTimer <= 0f)
            {
                gameManager.RequestPhaseChange(GameStateType.Playing);
            }
        }
    }
}
