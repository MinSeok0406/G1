using UnityEngine;
using UnityEngine.EventSystems;

namespace Minseok
{
    public class MouseStateController : MonoBehaviour
    {
        public Transform maxDragBounds;
        public Transform minDragBounds;

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


        #region Validate
#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateCheck.ValidateCheckNullValue(this, nameof(maxDragBounds), maxDragBounds);
            ValidateCheck.ValidateCheckNullValue(this, nameof(minDragBounds), minDragBounds);
        }
#endif
        #endregion
    }
}