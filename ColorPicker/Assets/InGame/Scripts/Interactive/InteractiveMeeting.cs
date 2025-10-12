using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class InteractiveMeeting : InteractiveTriggerBase
    {
        [SerializeField] private SpriteRenderer bodyRenderer; // 시체 색상
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
            // 필요 시 조건 추가(예: 이미 미팅 중이면 금지)
            return true;
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
