using System;
using UnityEngine.EventSystems;

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

        stateMachine.ChangeState(mouseStateController.idleState);
    }

    public override void Exit()
    {
        base.Exit();

        eventData.pointerCurrentRaycast = new RaycastResult();
    }
}

