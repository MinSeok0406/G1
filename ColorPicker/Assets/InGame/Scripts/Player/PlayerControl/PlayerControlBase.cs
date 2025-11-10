#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // Gamepad.current, Keyboard.current, Touchscreen.current
#endif

using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 플레이어 입력 및 이동을 처리하는 추상 베이스 클래스
    /// 공통 입력 처리 로직 제공
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public abstract class PlayerControlBase : MonoBehaviourPun
    {
        [SerializeField] private bl_Joystick _joystick;
        protected PlayerBase _player;
        protected CircleCollider2D _circleCollider;
        protected float _moveSpeed;
        protected bool _isControlDisabled;

        // Properties
        public bool IsControlDisabled => _isControlDisabled;
        public CircleCollider2D CircleCollider => _circleCollider;

        protected virtual void Awake()
        {
            InitializeComponents();
            CacheSettings();
        }

        protected virtual void Update()
        {
            if (!CanProcessInput()) return;

            ProcessInput();
        }

        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        protected virtual void InitializeComponents()
        {
            _player = GetComponent<PlayerBase>();
            _joystick = GetComponent<bl_Joystick>();
            _circleCollider = GetComponentInChildren<CircleCollider2D>(includeInactive: true);

            ValidateComponents();
        }

        /// <summary>
        /// 설정 값 캐싱
        /// </summary>
        protected virtual void CacheSettings()
        {
            _moveSpeed = Settings.moveSpeed;
        }

        /// <summary>
        /// 필수 컴포넌트 검증
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected virtual void ValidateComponents()
        {
            if (_player == null)
            {
                Debug.LogError($"[{GetType().Name}] PlayerBase 컴포넌트를 찾을 수 없습니다.", this);
            }

            if (_circleCollider == null)
            {
                Debug.LogWarning($"[{GetType().Name}] CircleCollider2D를 찾을 수 없습니다.", this);
            }
        }

        /// <summary>
        /// 입력 처리 가능 여부 확인
        /// </summary>
        protected virtual bool CanProcessInput()
        {
            if (!photonView.IsMine) return false;
            if (_isControlDisabled) return false;
            if (_player == null) return false;

            return true;
        }

        /// <summary>
        /// 입력 처리 (자식 클래스에서 구현)
        /// </summary>
        protected abstract void ProcessInput();

        /// <summary>
        /// 이동 입력 처리 (공통 로직)
        /// </summary>
        protected void HandleMovementInput()
        {
            Vector2 inputDirection = GetMovementInput();

            if (inputDirection != Vector2.zero)
            {
                Move(inputDirection);
            }
            else
            {
                Idle();
            }
        }

        /// <summary>
        /// 이동 입력 벡터 가져오기
        /// </summary>
        protected virtual Vector2 GetMovementInput()
        {
            // 1) 우선순위: 모바일/온스크린 조이스틱
            if (_joystick != null)
            {
                var j = new Vector2(_joystick.Horizontal, _joystick.Vertical);
                if (j.sqrMagnitude > 0.0001f)
                {
                    return Vector2.ClampMagnitude(j, 1f);
                }
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // 2) 새 Input System 읽기 (키보드/패드)
            var move = Vector2.zero;

            // 키보드 WASD/화살표
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = 0f, y = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                move += new Vector2(x, y);
            }

            // 게임패드 좌스틱
            if (Gamepad.current != null)
                move += Gamepad.current.leftStick.ReadValue();

            return Vector2.ClampMagnitude(move, 1f);
#else
    // 3) 레거시 입력(구 Input Manager)로 빌드할 때만 사용
    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");
    return new Vector2(h, v);
#endif
        }

        /// <summary>
        /// 이동 실행
        /// </summary>
        protected virtual void Move(Vector2 direction)
        {
            _player?.MovementByVelocityEvent?.CallMovementByVElocityEvent(direction, _moveSpeed);
        }

        /// <summary>
        /// 대기 상태로 전환
        /// </summary>
        protected virtual void Idle()
        {
            _player?.IdleEvent?.CallIdleEvent();
        }

        /// <summary>
        /// 플레이어 컨트롤 활성화/비활성화
        /// </summary>
        public virtual void SetControlEnabled(bool enabled)
        {
            _isControlDisabled = !enabled;

            if (_isControlDisabled)
            {
                Idle();
            }
        }

        /// <summary>
        /// 이동 속도 설정
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = Mathf.Max(0f, speed);
        }
    }
}