
namespace ColorPicker.InGame
{
    public class GameStartedState : GameState
    {
        public GameStartedState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            GameManager.Instance.InitializedGameManager();

            GameManager.Instance.S_AssignPlayerClasses();

            NetworkManager.Instance.MyPlayer.transform.position = Settings.playerSpawnPointInGame;

            GameManager.Instance.stateMachine.ChangeState(GameManager.Instance.playingGameState);
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
