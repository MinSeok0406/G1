using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 시체 발견 인터랙티브 오브젝트
    /// 플레이어가 시체에 접근하면 시체 신고 가능
    /// </summary>
    public class InteractiveDeadBody : InteractiveTriggerBase
    {
        [Header("Dead Body Info")]
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private int deadPlayerActorId = -1; // 죽은 플레이어의 Actor ID

        private bool _isReported = false;

        protected override void Awake()
        {
            base.Awake();

            // 초기 아이콘 상태
            if (bodySprite)
            {
                bodySprite.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 시체 초기화 (DeathBodyManager나 Death 클래스에서 호출)
        /// </summary>
        public void Initialize(int actorId, Color bodyColor)
        {
            deadPlayerActorId = actorId;

            if (bodySprite != null)
            {
                bodySprite.color = bodyColor;
            }

            _isReported = false;

            Debug.Log($"[InteractiveDeadBody] Initialized for actor {actorId}");
        }

        protected override bool CanInteract()
        {
            // 이미 신고된 시체는 상호작용 불가
            if (_isReported)
                return false;

            // 내 플레이어가 살아있는지 확인
            int myActorId = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorId < 0)
                return false;

            if (!GameDataManager.Instance.TryGetInGameDataByActorId(myActorId, out var myInGameData))
                return false;

            // 죽은 플레이어는 시체 신고 불가
            if (!myInGameData.isAlive)
                return false;

            return true;
        }

        protected override void HandleInteractLocal()
        {
            if (_isReported)
                return;

            int myActorId = Photon.Pun.PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorId < 0)
            {
                Debug.LogWarning("[InteractiveDeadBody] Cannot get local player actor ID.");
                return;
            }

            // 시체 신고
            ReportDeadBody(myActorId);
        }

        /// <summary>
        /// 시체 신고
        /// </summary>
        private void ReportDeadBody(int reporterActorId)
        {
            _isReported = true;

            Debug.Log($"[InteractiveDeadBody] Player {reporterActorId} reported dead body of player {deadPlayerActorId} at {transform.position}");

            // MeetingManager에 시체 발견 미팅 요청
            if (MeetingManager.Instance != null)
            {
                MeetingManager.Instance.RequestBodyReportMeeting(reporterActorId, transform.position);
            }
            else
            {
                Debug.LogWarning("[InteractiveDeadBody] MeetingManager not found.");
            }

            // 상호작용 정리
            CleanupLocalInteraction();

            // 트리거 비활성화 (중복 신고 방지)
            if (triggerCollider)
                triggerCollider.enabled = false;
        }

        /// <summary>
        /// 시체가 신고되었는지 여부 반환
        /// </summary>
        public bool IsReported() => _isReported;

        /// <summary>
        /// 죽은 플레이어의 Actor ID 반환
        /// </summary>
        public int GetDeadPlayerActorId() => deadPlayerActorId;
    }
}
