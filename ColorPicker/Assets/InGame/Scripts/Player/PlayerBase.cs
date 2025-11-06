using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Idle))]
    [RequireComponent(typeof(IdleEvent))]
    [RequireComponent(typeof(MovementByVelocity))]
    [RequireComponent(typeof(MovementByVelocityEvent))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public abstract class PlayerBase : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [Header("Customize Settings")]
        [SerializeField] private SpriteRenderer[] customizableRenderers; // 색상을 변경할 렌더러 배열

        // Lazy-loaded components (캐싱)
        private IdleEvent _idleEvent;
        private MovementByVelocityEvent _movementByVelocityEvent;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private PlayerControlBase _playerControl;
        private Rigidbody2D _rigidbody;

        // Public properties (Lazy Loading)
        public IdleEvent IdleEvent => _idleEvent ??= GetComponent<IdleEvent>();
        public MovementByVelocityEvent MovementByVelocityEvent => _movementByVelocityEvent ??= GetComponent<MovementByVelocityEvent>();
        public Animator Animator => _animator ??= GetComponent<Animator>();
        public SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponent<SpriteRenderer>();
        public PlayerControlBase PlayerControl => _playerControl ??= GetComponent<PlayerControlBase>();
        public Rigidbody2D Rigidbody => _rigidbody ??= GetComponent<Rigidbody2D>();

        // Properties
        public bool IsLocalPlayer => photonView != null && photonView.IsMine;
        public int OwnerActorNumber => photonView?.OwnerActorNr ?? -1;

        // Customize color
        private Color _customizeColor = Color.white;

        protected virtual void Awake()
        {
            ValidateComponents();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        /// <summary>
        /// 필수 컴포넌트 검증 (에디터에서만 실행)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void ValidateComponents()
        {
            if (PlayerControl == null)
            {
                Debug.LogError($"[{GetType().Name}] PlayerControl 컴포넌트가 없습니다.", this);
            }
        }

        /// <summary>
        /// 플레이어 초기화 (템플릿 메서드 패턴)
        /// </summary>
        public void InitializePlayer()
        {
            if (!IsLocalPlayer)
            {
                Debug.LogWarning($"[{GetType().Name}] 로컬 플레이어가 아닌 경우 초기화할 수 없습니다.");
                return;
            }

            OnBeforeInitialize();
            PerformInitialization();
            OnAfterInitialize();
        }

        /// <summary>
        /// 초기화 전 훅 (오버라이드 가능)
        /// </summary>
        protected virtual void OnBeforeInitialize() { }

        /// <summary>
        /// 실제 초기화 로직 (자식 클래스에서 구현)
        /// </summary>
        protected abstract void PerformInitialization();

        /// <summary>
        /// 초기화 후 훅 (오버라이드 가능)
        /// </summary>
        protected virtual void OnAfterInitialize() { }

        #region Photon Ownership Callbacks

        public virtual void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer) { }

        public virtual void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            if (targetView != photonView) return;
            if (!IsLocalPlayer) return;
            if (PhotonNetwork.IsMasterClient) return;

            InitializePlayer();
        }

        public virtual void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player senderOfFailedRequest)
        {
            Debug.LogWarning($"[{GetType().Name}] 소유권 이전 실패: {senderOfFailedRequest?.NickName}", this);
        }

        #endregion

        #region Customize Color

        /// <summary>
        /// 커스터마이즈 색상 적용
        /// </summary>
        public virtual void ApplyCustomizeColor(Color color)
        {
            _customizeColor = color;

            // customizableRenderers가 할당되어 있지 않으면 적용하지 않음
            // (특정 렌더러에만 색상을 적용하기 위함)
            if (customizableRenderers == null || customizableRenderers.Length == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] customizableRenderers is not assigned. Color will not be applied.", this);
                return;
            }

            // 지정된 렌더러들에만 색상 적용
            int appliedCount = 0;
            foreach (var renderer in customizableRenderers)
            {
                if (renderer != null)
                {
                    renderer.color = color;
                    appliedCount++;
                }
            }

            Debug.Log($"[{GetType().Name}] Applied color (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2}) to {appliedCount} renderer(s)", this);
        }

        /// <summary>
        /// 현재 커스터마이즈 색상 가져오기
        /// </summary>
        public Color GetCustomizeColor()
        {
            return _customizeColor;
        }

        #endregion
    }
}