using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 인게임에서의 플레이어 컨트롤
    /// 상호작용 시스템 포함
    /// </summary>
    public sealed class PlayerControl : PlayerControlBase
    {
        private InteractionDetector _interactionDetector;
        private Player _inGamePlayer;
        private PlayerInteractionHandler _interactionHandler;

        // 현재 상호작용 가능한 오브젝트 (InteractionDetector가 설정)
        public IInteractive CurrentInteractive
        {
            get => _interactionHandler?.CurrentInteractive;
            set => _interactionHandler?.SetCurrentInteractive(value);
        }

        public InteractionDetector InteractionDetector => _interactionDetector;

        protected override void Awake()
        {
            base.Awake();
            InitializeInGameComponents();
        }

        private void OnEnable()
        {
            UpdateInteractionDetectorState();
        }

        /// <summary>
        /// 인게임 전용 컴포넌트 초기화
        /// </summary>
        private void InitializeInGameComponents()
        {
            _inGamePlayer = _player as Player;
            _interactionDetector = GetComponentInChildren<InteractionDetector>(includeInactive: true);
            _interactionHandler = new PlayerInteractionHandler();

            ValidateInGameComponents();
            UpdateInteractionDetectorState();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void ValidateInGameComponents()
        {
            if (_interactionDetector == null)
            {
                Debug.LogWarning("[PlayerControl] InteractionDetector를 찾을 수 없습니다.", this);
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

        protected override bool CanProcessInput()
        {
            if (!base.CanProcessInput()) return false;

            // 인게임 플레이어 추가 검증
            if (_inGamePlayer != null)
            {
                int ownerActorNum = _inGamePlayer.OwnerActNum;
                int localActorNum = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;

                if (ownerActorNum != localActorNum)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void ProcessInput()
        {
            HandleMovementInput();
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
    }

    /// <summary>
    /// 플레이어 상호작용 처리 핸들러
    /// 단일 책임 원칙(SRP) 적용
    /// </summary>
    internal sealed class PlayerInteractionHandler
    {
        private IInteractive _currentInteractive;

        public IInteractive CurrentInteractive => _currentInteractive;

        /// <summary>
        /// 상호작용 시도
        /// </summary>
        public void TryInteract()
        {
            if (!IsInteractionValid())
            {
                return;
            }

            ExecuteInteraction();
        }

        /// <summary>
        /// 상호작용 유효성 검증
        /// </summary>
        private bool IsInteractionValid()
        {
            if (_currentInteractive == null)
            {
                return false;
            }

            // Unity Object null 체크 (Destroyed된 경우 대응)
            if (_currentInteractive is Object unityObject && unityObject == null)
            {
                ClearInteraction();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 상호작용 실행
        /// </summary>
        private void ExecuteInteraction()
        {
            try
            {
                _currentInteractive.OnInteract();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerInteractionHandler] 상호작용 실행 중 오류: {ex.Message}");
                ClearInteraction();
            }
        }

        /// <summary>
        /// 현재 상호작용 대상 설정
        /// </summary>
        public void SetCurrentInteractive(IInteractive target)
        {
            _currentInteractive = target;
        }

        /// <summary>
        /// 상호작용 정보 클리어
        /// </summary>
        public void ClearInteraction()
        {
            _currentInteractive = null;
        }
    }
}