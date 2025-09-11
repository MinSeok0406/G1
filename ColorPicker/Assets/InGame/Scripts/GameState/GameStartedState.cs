
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using UnityEngine;

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

            if (!PhotonNetwork.IsMasterClient) return;

            ClientAckManager.Instance.WaitForAllClients(() => {Initialized(); });
            
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();
        }

        private void Initialized()
        {
            GameManager.Instance.playerClassAssigner.AssignRoles();

            GameDataManager.Instance.InitializedPlayerInGameData();

            UIManager.Instance.InitializedPlayerProfile();

            GameManager.Instance.RequestPhaseChange(GameStateType.Playing);
        }
    }
}
