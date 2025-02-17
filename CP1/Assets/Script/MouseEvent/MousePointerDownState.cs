using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MousePointerDownState: MouseState
{
    private Vector2 previousPosition;

    public MousePointerDownState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine) : base(mouseStateController, eventData, mouseStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        Vector2 previousPosition = eventData.position;
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(0))
        {
            DetectDrag();
        }
        else
        {
            stateMachine.ChangeState(mouseStateController.pointerUpState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void DetectDrag()
    {
        UpdateEventDataPosition();

        if(Vector2.Distance(previousPosition, eventData.position) > 0.5f)
        {
            stateMachine.ChangeState(mouseStateController.dragState);
        }
    }
}

