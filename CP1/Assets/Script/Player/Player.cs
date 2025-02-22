using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
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
public class Player : MonoBehaviour
{
    public int Id { get; set; }
    public bool myPlayer = false;

    public bool _updated = false;

    PositionInfo _positionInfo = new PositionInfo();
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

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;

    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        idleEvent = GetComponent<IdleEvent>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        Vector3 pos = new Vector3(0.5f, 0.5f);
        transform.position = pos;

        CellPos = new Vector3(0.0f, 0.0f);
    }
}
