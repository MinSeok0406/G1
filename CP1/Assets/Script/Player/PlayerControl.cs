using Google.Protobuf.Protocol;
using UnityEngine;

/*#region RequireComponent
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
#endregion*/

//[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    public static PlayerControl Instance { get; } = new PlayerControl();

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
    [HideInInspector] public Animator animator;

    [SerializeField]
    public float moveSpeed;

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
        }
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
            UpdateAnimation();
            _updated = true;
        }
    }

    protected MoveDir _lastDir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;
            if (value != MoveDir.None)
                _lastDir = value;

            UpdateAnimation();
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
        //if 조건문 조정 후 CallIdleEvent, CallMovementEvent 넣을 예정 
        UpdateController();
        transform.position = new Vector3(PosInfo.PosX, PosInfo.PosY, 0);
    }

    protected virtual void Init()
    {
        #region GetComponet
        rb = GetComponent<Rigidbody2D>();
        idleEvent = GetComponent<IdleEvent>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        animator = GetComponent<Animator>();
        #endregion

        //Vector3 pos = new Vector3(0, 0, 0);
        //transform.position = pos;

        moveSpeed = Settings.playerMoveSpeed;
        State = CreatureState.Idle;
        Dir = MoveDir.None;
        CellPos = new Vector3(0, 0, 0);
        UpdateAnimation();
    }

    protected virtual void UpdateAnimation()
    {
        if (State == CreatureState.Idle)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    //_animator.Play("IDLE_BACK");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    //_animator.Play("IDLE_FRONT");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    //_animator.Play("IDLE_RIGHT");
                    //_sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    //_animator.Play("IDLE_RIGHT");
                    //_sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    //_animator.Play("WALK_BACK");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    //_animator.Play("WALK_FRONT");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    //_animator.Play("WALK_RIGHT");
                    //_sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    //_animator.Play("WALK_RIGHT");
                    //_sprite.flipX = false;
                    break;
            }
        }
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
        if (Dir != MoveDir.None)
        {
            State = CreatureState.Moving;
            return;
        }
    }

    protected virtual void UpdateDead()
    {

    }

    protected virtual void UpdateMoving()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

        if (horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction = direction.normalized;
        }

        if (direction != Vector2.zero)
        {
            movementByVelocityEvent.CallMovementByVelocity(direction, moveSpeed);
        }
        else
        {
            idleEvent.CallIdleEvent();
        }

        MoveToNextPos();
    }

    protected virtual void MoveToNextPos()
    {

    }
}
