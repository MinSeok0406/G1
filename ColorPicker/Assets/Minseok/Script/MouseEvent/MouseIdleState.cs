using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    public class MouseIdleState : MouseState
    {
        public MouseIdleState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine) : base(mouseStateController, eventData, mouseStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetMouseButtonDown(0))
            {
                UpdateEventDataPosition();

                if (GetFirstRayCastHit())
                {
                    stateMachine.ChangeState(mouseStateController.pointerDownState);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}