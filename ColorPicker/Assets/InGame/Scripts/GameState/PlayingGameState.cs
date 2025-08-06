
using Photon.Pun;
using System.Diagnostics;

namespace ColorPicker.InGame
{
    public class PlayingGameState : GameState
    {
        public PlayingGameState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            if (!PhotonNetwork.IsMasterClient) return;

            MissionManager.Instance.InitializePlayerMissions(GameDataManager.Instance.GetAllPublicPlayerData());
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
