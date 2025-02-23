using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerControl : PlayerControl
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
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

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (Dir != MoveDir.None)
        {
            State = CreatureState.Moving;
            return;
        }
    }

    // 키보드 입력
    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
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

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3.up * moveSpeed * Time.unscaledDeltaTime;
                break;
            case MoveDir.Down:
                destPos += Vector3.down * moveSpeed * Time.unscaledDeltaTime;
                break;
            case MoveDir.Left:
                destPos += Vector3.left * moveSpeed * Time.unscaledDeltaTime;
                break;
            case MoveDir.Right:
                destPos += Vector3.right * moveSpeed * Time.unscaledDeltaTime;
                break;
        }

        if (Managers.Object.Find(destPos) == null)
        {
            CellPos = destPos;
        }

        CheckUpdatedFlag();
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
