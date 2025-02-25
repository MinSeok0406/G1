using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerControl : PlayerControl
{
    private Vector3 moveDirection;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }
    
    // 키보드 입력
    private void GetDirInput()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontalMovement, verticalMovement, 0);

        if (horizontalMovement != 0f && verticalMovement != 0f)
        {
            moveDirection = moveDirection.normalized;
        }

        if(moveDirection != Vector3.zero)
        {
            float angle = HelperUtilities.GetAngleFromVector(moveDirection);
            Dir = HelperUtilities.GetMoveDirection(angle);
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    protected override void MoveToNextPos()
    {
        if (Dir == MoveDir.None)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }

        Vector3 destPos = CellPos;

        destPos += moveDirection * moveSpeed * Time.unscaledDeltaTime;

        if (Managers.Object.Find(destPos) == null)
        {
            CellPos = destPos;
        }

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
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
