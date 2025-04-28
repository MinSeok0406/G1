using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    public class MouseDragState : MouseState
    {
        private PRS prs;
        private Vector3 offset;
        private MouseInteractiveObject mouseInterectiveObject;

        public MouseDragState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine) : base(mouseStateController, eventData, mouseStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            mouseInterectiveObject = GetInterectiveObject();
            prs = mouseInterectiveObject.originPRS;
            offset = prs.pos - HelperUtilities.GetMouseWorldPosition(eventData);
        }

        public override void Update()
        {
            base.Update();

            UpdateEventDataPosition();

            if (Input.GetMouseButton(0))
            {
                prs.pos = HelperUtilities.GetMouseWorldPosition(eventData) + offset;

                prs.pos.x = Mathf.Clamp(prs.pos.x, mouseStateController.minDragBounds.position.x, mouseStateController.maxDragBounds.position.x);
                prs.pos.y = Mathf.Clamp(prs.pos.y, mouseStateController.minDragBounds.position.y, mouseStateController.maxDragBounds.position.y);

                mouseInterectiveObject?.MoveTransform(prs, 0);

            }
            else
            {
                stateMachine.ChangeState(mouseStateController.pointerUpState);
            }

        }

        public override void Exit()
        {
            base.Exit();

            prs = null;
        }
    }
}