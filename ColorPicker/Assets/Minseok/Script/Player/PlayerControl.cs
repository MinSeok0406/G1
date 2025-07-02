using Google.Protobuf.Protocol;
using UnityEngine;

namespace Minseok
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerControl : MonoBehaviour
    {
        public static PlayerControl Instance { get; } = new PlayerControl();

        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer sprite;
        private Rigidbody2D _rb;
        private Collider2D _co;

        private PositionInfo _positionInfo = new PositionInfo();
        protected bool _updated = false;

        #region property
        public int Id { get; set; }

        public PositionInfo PosInfo
        {
            get { return _positionInfo; }
            set
            {
                if (_positionInfo.Equals(value))
                    return;

                CellPos = new Vector3(value.PosX, value.PosY, 0);
                State = value.State;
                Dir = value.MoveDir;
                Speed = value.Speed;
            }
        }

        public void SyncPos()
        {
            Vector3 destPos = new Vector3(0, 0);
            transform.position = destPos;
        }

        public Vector3 CellPos
        {
            get
            {
                return new Vector3(PosInfo.PosX, PosInfo.PosY, 0);
            }

            set
            {
                if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                    return;

                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
                _updated = true;
            }
        }

        public virtual CreatureState State
        {
            get { return PosInfo.State; }
            set
            {
                if (PosInfo.State == value)
                    return;

                PosInfo.State = value;
                _updated = true;
            }
        }

        public float Speed
        {
            get { return PosInfo.Speed; }
            set
            {
                if (PosInfo.Speed == value)
                    return;

                PosInfo.Speed = value;
                _updated = true;
            }
        }

        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set
            {
                if (PosInfo.MoveDir == value)
                    return;

                PosInfo.MoveDir = value;
                _updated = true;
            }
        }
        #endregion

        void Awake()
        {
            Init();
        }

        protected void Update()
        {
            UpdateController();
            transform.position = new Vector3(PosInfo.PosX, PosInfo.PosY, 0);
        }

        protected virtual void Init()
        {
            #region GetComponet

            _rb = GetComponent<Rigidbody2D>();
            _co = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            #endregion

            State = CreatureState.Idle;
            Dir = MoveDir.None;
            CellPos = new Vector3(0, 0, 0);
        }

        protected virtual void UpdateController()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

        protected virtual void UpdateIdle()
        {
            if (animator == null)
                return;

            // 이동 상태로 갈지 확인
            if (Dir != MoveDir.None)
            {
                State = CreatureState.Moving;
                return;
            }

            _rb.velocity = Vector2.zero;
        }

        protected virtual void UpdateMoving()
        {
            if (animator == null)
                return;

            if (Dir == MoveDir.None)
            {
                State = CreatureState.Idle;
            }

            Vector2 dir = Managers.Game.JoystickDir;
            Vector2 moveDir = new Vector2(dir.x, dir.y);
            moveDir = moveDir.normalized;

            if (moveDir != Vector2.zero)
            {
                _rb.velocity = moveDir * Time.deltaTime * Speed;
                State = CreatureState.Moving;
            }

            MoveToNextPos();
        }

        protected virtual void UpdateDead()
        {
            if (animator == null)
                return;


        }

        protected virtual void MoveToNextPos()
        {

        }

        protected virtual void CheckUpdatedFlag()
        {

        }
    }
}