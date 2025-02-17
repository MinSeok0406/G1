using UnityEngine;
using UnityEngine.EventSystems;

public class MouseStateController : MonoBehaviour
{
    protected PointerEventData eventData;

    #region State
    public MouseStateMachine stateMachine { get; private set; }

    public MouseIdleState idleState { get; private set; }
    public MousePointerDownState pointerDownState { get; private set; }
    public MousePointerUpState pointerUpState { get; private set; }
    public MouseDragState dragState { get; private set; }
    #endregion

    private void Awake()
    {
        eventData = new PointerEventData(EventSystem.current);

        stateMachine = new MouseStateMachine();

        idleState = new MouseIdleState(this, eventData, stateMachine);
        pointerDownState = new MousePointerDownState(this, eventData, stateMachine);
        pointerUpState = new MousePointerUpState(this, eventData, stateMachine);
        dragState = new MouseDragState(this, eventData, stateMachine);
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.currentState.Update();
    }
}

