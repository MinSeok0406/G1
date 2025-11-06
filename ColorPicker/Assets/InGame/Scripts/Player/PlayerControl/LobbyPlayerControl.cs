using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 로비에서의 플레이어 컨트롤
    /// Tab 키로 플레이어 슬롯 UI 표시/숨김 기능 포함
    /// 상호작용 시스템 포함
    /// </summary>
    public sealed class LobbyPlayerControl : PlayerControlBase
    {
        private LobbyPlayerSlotUIHandler _slotUIHandler;
        private InteractionDetector _interactionDetector;
        private PlayerInteractionHandler _interactionHandler;

        // 현재 상호작용 가능한 오브젝트
        public IInteractive CurrentInteractive
        {
            get => _interactionHandler?.CurrentInteractive;
            set => _interactionHandler?.SetCurrentInteractive(value);
        }

        public InteractionDetector InteractionDetector => _interactionDetector;

        protected override void Awake()
        {
            base.Awake();
            _slotUIHandler = new LobbyPlayerSlotUIHandler();
            InitializeInteractionSystem();
        }

        private void OnEnable()
        {
            UpdateInteractionDetectorState();
        }

        /// <summary>
        /// 상호작용 시스템 초기화
        /// </summary>
        private void InitializeInteractionSystem()
        {
            _interactionDetector = GetComponentInChildren<InteractionDetector>(includeInactive: true);
            _interactionHandler = new PlayerInteractionHandler();

            ValidateInteractionComponents();
            UpdateInteractionDetectorState();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void ValidateInteractionComponents()
        {
            if (_interactionDetector == null)
            {
                Debug.LogWarning("[LobbyPlayerControl] InteractionDetector를 찾을 수 없습니다.", this);
            }
        }

        /// <summary>
        /// 인터랙션 디텍터 활성화 상태 업데이트
        /// </summary>
        private void UpdateInteractionDetectorState()
        {
            if (_interactionDetector != null)
            {
                _interactionDetector.enabled = photonView.IsMine;
            }
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

        /// <summary>
        /// 상호작용 시도
        /// </summary>
        public void TryInteract()
        {
            _interactionHandler?.TryInteract();
        }

        /// <summary>
        /// 현재 상호작용 대상 설정 (외부 호출용)
        /// </summary>
        public void SetCurrentInteractive(IInteractive target)
        {
            _interactionHandler?.SetCurrentInteractive(target);
        }

        public override void SetControlEnabled(bool enabled)
        {
            base.SetControlEnabled(enabled);

            // 컨트롤 비활성화 시 상호작용도 클리어
            if (!enabled)
            {
                _interactionHandler?.ClearInteraction();
            }
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