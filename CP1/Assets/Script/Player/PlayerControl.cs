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
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
    [HideInInspector] public Animator animator;

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

            CellPos = new Vector2(value.PosX, value.PosY);
            //State = value.State;
            //Dir = value.MoveDir;
        }
    }
    public Vector3 CellPos
    {
        get
        {
            return new Vector3(PosInfo.PosX, PosInfo.PosY);
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
    #endregion

    void Awake()
    {
        Init();
    }

    private void Update()
    {
        //if 조건문 조정 후 CallIdleEvent, CallMovementEvent 넣을 예정 
        
        transform.position = new Vector3(PosInfo.PosX, PosInfo.PosY);
    }

    protected virtual void Init()
    {
        #region GetComponet
        rb = GetComponent<Rigidbody2D>();
        idleEvent = GetComponent<IdleEvent>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        animator = GetComponent<Animator>();
        #endregion

        Vector3 pos = new Vector3(0, 0);
        transform.position = pos;

        CellPos = new Vector3(0.0f, 0.0f);
    }
}
