
namespace Minseok
{
    public class MouseStateMachine
    {
        public MouseState currentState { get; private set; }

        public void Initialize(MouseState startState)
        {
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(MouseState newState)
        {
            currentState.Exit();
            currentState = newState;
            currentState.Enter();
        }
    }
}