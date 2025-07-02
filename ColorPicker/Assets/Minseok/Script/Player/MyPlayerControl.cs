using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MyPlayerControl : PlayerControl
    {
        protected override void Init()
        {
            base.Init();
        }

        protected override void UpdateController()
        {
            GetUIKeyInput();

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

        void LateUpdate()
        {
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }

        // 키보드 입력
        private void GetDirInput()
        {
            Vector2 dir = Managers.Game.JoystickDir;
            Vector2 moveDir = new Vector2(dir.x, dir.y);
            moveDir = moveDir.normalized;

            if (moveDir != Vector2.zero)
            {
                float angle = HelperUtilities.GetAngleFromVector(moveDir);
                Dir = HelperUtilities.GetMoveDirection(angle);
            }
            else
            {
                Dir = MoveDir.None;
            }
        }

        void GetUIKeyInput()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
                UI_ChatScene chatUI = gameSceneUI.ChatUI;

                if (chatUI.gameObject.activeSelf)
                {
                    chatUI.gameObject.SetActive(false);
                }
                else
                {
                    chatUI.gameObject.SetActive(true);
                }
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

            Vector2 dir = Managers.Game.JoystickDir;
            Vector2 moveDir = new Vector2(dir.x, dir.y);
            moveDir = moveDir.normalized;

            Vector2 destPos = CellPos;

            destPos += moveDir * Speed * Time.unscaledDeltaTime;

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
}