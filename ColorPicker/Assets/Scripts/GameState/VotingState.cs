
namespace ColorPicker.InGame
{
    public class VotingState : GameState
    {
        public VotingState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            NetworkManager.Instance.MyPlayer.playerControl.DisablePlayerControl(true);
        }

        public override void Exit()
        {
            base.Exit();

            NetworkManager.Instance.MyPlayer.playerControl.DisablePlayerControl(false);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
