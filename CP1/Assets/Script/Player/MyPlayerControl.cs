using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerControl : Player
{
    protected override void Init()
    {
        base.Init();

        moveSpeed = Settings.playerMoveSpeed;
    }

    private float moveSpeed;


    private void Update()
    {
        CheckUpdatedFlag();

        MovementInput();
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
            GetComponent<MovementByVelocityEvent>().CallMovementByVelocity(direction, moveSpeed);
        }
        else
        {
            GetComponent<IdleEvent>().CallIdleEvent();
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
