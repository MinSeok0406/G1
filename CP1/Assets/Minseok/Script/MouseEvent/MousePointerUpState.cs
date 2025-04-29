using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    public class MousePointerUpState : MouseState
    {
        public MousePointerUpState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine) : base(mouseStateController, eventData, mouseStateMachine)
        {

        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            if (!Input.GetMouseButton(0))
            {
                stateMachine.ChangeState(mouseStateController.idleState);
            }
        }

        public override void Exit()
        {
            base.Exit();

            GetInterectiveObject().mouseInteractiveEvent.CallPointerUpEvent();

            eventData.pointerCurrentRaycast = new RaycastResult();
        }
    }
}