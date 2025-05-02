using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    public class MousePointerDownState : MouseState
    {
        private Vector2 previousPosition;
        MouseInteractiveObject mouseInteractiveObject;

        public MousePointerDownState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine) : base(mouseStateController, eventData, mouseStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Vector2 previousPosition = eventData.position;

            mouseInteractiveObject = GetInterectiveObject();
            mouseInteractiveObject?.mouseInteractiveEvent.CallPointerDownEvent();
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetMouseButton(0))
            {
                if (mouseInteractiveObject.mouseInteractive == MouseInteractive.Drag)
                {
                    DetectDrag();
                }
                else
                {
                    stateMachine.ChangeState(mouseStateController.pointerUpState);
                    return;
                }
            }
            else
            {
                stateMachine.ChangeState(mouseStateController.pointerUpState);
            }
        }

        public override void Exit()
        {
            base.Exit();

            mouseInteractiveObject = null;
        }

        private void DetectDrag()
        {
            UpdateEventDataPosition();

            if (Vector2.Distance(previousPosition, eventData.position) > 0.5f)
            {
                stateMachine.ChangeState(mouseStateController.dragState);
            }
        }
    }
}