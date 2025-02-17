using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDragState : MouseState
{
    private PRS prs;
    private Vector3 offset;
    private MouseInterectiveObject mouseInterectiveObject;

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

