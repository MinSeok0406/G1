using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 로비에서의 플레이어 컨트롤
    /// Tab 키로 플레이어 슬롯 UI 표시/숨김 기능 포함
    /// </summary>
    public sealed class LobbyPlayerControl : PlayerControlBase
    {
        private LobbyPlayerSlotUIHandler _slotUIHandler;

        protected override void Awake()
        {
            base.Awake();
            _slotUIHandler = new LobbyPlayerSlotUIHandler();
        }

        protected override void ProcessInput()
        {
            HandleMovementInput();
            HandleSlotUIInput();
        }

        /// <summary>
        /// 플레이어 슬롯 UI 입력 처리
        /// </summary>
        private void HandleSlotUIInput()
        {
            bool isTabPressed = Input.GetKey(KeyCode.Tab);
            _slotUIHandler?.UpdateVisibility(isTabPressed);
        }

        private void OnDestroy()
        {
            _slotUIHandler?.Cleanup();
        }
    }

    /// <summary>
    /// 로비 플레이어 슬롯 UI 표시 핸들러
    /// 단일 책임 원칙(SRP) 적용
    /// </summary>
    internal sealed class LobbyPlayerSlotUIHandler
    {
        private bool _isSlotVisible;

        /// <summary>
        /// UI 가시성 업데이트
        /// </summary>
        public void UpdateVisibility(bool shouldBeVisible)
        {
            if (_isSlotVisible == shouldBeVisible) return;

            _isSlotVisible = shouldBeVisible;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (LobbyUIManager.Instance == null)
            {
                Debug.LogWarning("[LobbyPlayerSlotUIHandler] LobbyUIManager 인스턴스를 찾을 수 없습니다.");
                return;
            }

            LobbyUIManager.Instance.SetPlayerSlotsActive(_isSlotVisible);
        }

        /// <summary>
        /// 정리
        /// </summary>
        public void Cleanup()
        {
            if (_isSlotVisible)
            {
                UpdateVisibility(false);
            }
        }
    }
}