using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MiniGameTag))]
    [DisallowMultipleComponent]
    public class MiniGameTrigger : InteractiveTriggerBase
    {
        private MiniGameTag _miniGame;

        protected override void Awake()
        {
            base.Awake();
            _miniGame = GetComponent<MiniGameTag>();
        }

        protected override bool CanInteract()
        {
            // 필요시 미니게임 가능 조건을 여기서 제한 (쿨다운/상태 등)
            return _miniGame != null && MiniGameManager.Instance != null;
        }

        protected override void HandleInteractLocal()
        {
            // 미니게임 시작(로컬 트리거)
            MiniGameManager.Instance.StartMiniGame(_miniGame.miniGameType);

            // 필요 시 버튼/하이라이트 유지/해제 정책 선택
            // 보통은 즉시 정리 (사용자 피드백 후 중복입력 방지)
            CleanupLocalInteraction();
        }
    }
}
