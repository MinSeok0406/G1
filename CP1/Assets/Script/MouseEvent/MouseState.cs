using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseState
{
    protected MouseStateMachine stateMachine;
    protected PointerEventData eventData;
    protected MouseStateController mouseStateController;

    public MouseState(MouseStateController mouseStateController, PointerEventData eventData, MouseStateMachine mouseStateMachine)
    {
        this.mouseStateController = mouseStateController;
        this.eventData = eventData;
        this.stateMachine = mouseStateMachine;
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {

    }

    protected void UpdateEventDataPosition()
    {
        eventData.position = Input.mousePosition;
    }

    protected bool GetFirstRayCastHit()
    {
        List<RaycastResult> raycastResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResult);

        if (raycastResult.Count > 0)
        {
            RaycastResult topHit = raycastResult.OrderBy(hit => hit.depth).Last();
            eventData.pointerCurrentRaycast = topHit;

            return true;
        }

        return false;
    }

    protected MouseInterectiveObject GetInterectiveObject()
    {
        return eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<MouseInterectiveObject>();
    }
}
