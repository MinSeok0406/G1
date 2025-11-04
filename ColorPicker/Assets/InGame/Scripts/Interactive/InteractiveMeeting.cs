using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class InteractiveMeeting : InteractiveTriggerBase
    {
        [SerializeField] private SpriteRenderer bodyRenderer; // 시체 색상
        [SerializeField, Range(0f, 1f)] private float requiredProgressRatio = 0.5f; // 회의 시작에 필요한 미션 진행률 (50%)

        private string ownerUID;   // 사망자 UID
        private int ownerActorId; // 사망자 ActorId

        // ===== 초기화 (호스트 전용) =====
        public void Initialize(string uid, int actorId, ColorType color)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ownerUID = uid;
            ownerActorId = actorId;
            SetBodyColor(color);
        }

        private void SetBodyColor(ColorType color)
        {
            if (bodyRenderer) bodyRenderer.color = HelperUtilities.ToUnityColor(color);
        }

        protected override bool CanInteract()
        {
            // 미션 진행률 확인
            if (!CheckMissionProgress())
            {
                UIManager.Instance?.ShowToastToScreen($"미션을 {requiredProgressRatio * 100}% 이상 완료해야 회의를 시작할 수 있습니다.");
                return false;
            }

            // 이미 미팅 중이면 금지
            var currentState = GameManager.Instance?.StateMachine?.CurrentState;
            if (currentState is MeetingState)
            {
                UIManager.Instance?.ShowToastToScreen("이미 회의가 진행 중입니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 전체 플레이어의 미션 진행률 확인
        /// </summary>
        private bool CheckMissionProgress()
        {
            if (GameDataManager.Instance == null) return false;

            var allPlayers = GameDataManager.Instance.GetAllPublicPlayerData();
            if (allPlayers == null || allPlayers.Count == 0) return false;

            int totalMissions = 0;
            int completedMissions = 0;

            // 모든 생존 플레이어의 미션 진행 상황 집계
            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                // 죽은 플레이어는 제외
                var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
                if (inGameData == null || !inGameData.isAlive) continue;

                // 미션 데이터 가져오기
                if (GameDataManager.Instance.TryGetMissionData(playerData.googleUID, out var missionData))
                {
                    if (missionData.missionList != null)
                    {
                        totalMissions += missionData.missionList.Count;

                        foreach (var mission in missionData.missionList)
                        {
                            if (mission.isCompleted)
                            {
                                completedMissions++;
                            }
                        }
                    }
                }
            }

            if (totalMissions == 0) return true; // 미션이 없으면 허용

            float progressRatio = (float)completedMissions / totalMissions;
            return progressRatio >= requiredProgressRatio;
        }

        protected override void HandleInteractLocal()
        {
            // 호스트 권한 동작은 GameManager 내부에서 검증한다고 가정
            GameManager.Instance.RequestPhaseChange(GameStateType.Meeting);

            // 상호작용 즉시 버튼/하이라이트 정리
            CleanupLocalInteraction();
        }
    }
}
