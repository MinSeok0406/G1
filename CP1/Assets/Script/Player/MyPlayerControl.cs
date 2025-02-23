using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerControl : PlayerControl
{
    private float moveSpeed;

    protected override void Init()
    {
        base.Init();

        moveSpeed = Settings.playerMoveSpeed;
    }

    private void Update()
    {
        MovementInput();

        CheckUpdatedFlag();
    }

    private void MovementInput()
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

        CellPos = transform.position;
    }

    void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;   
        }
    }
}
