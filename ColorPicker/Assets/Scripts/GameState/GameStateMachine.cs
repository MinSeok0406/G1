using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColorPicker.InGame
{
    public class GameStateMachine : MonoBehaviourPun
    {
        public GameState currentState { get; private set; }

        public void Initialize(GameState startState)
        {
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(GameState newState)
        {
            C_RequestGameStateChange(newState);
        }

        private void ChangeStateProcess(GameState newState)
        {
            currentState.Exit();
            currentState = newState;
            currentState.Enter();
        }

        private void C_RequestGameStateChange(GameState gameState)
        {
            int gameStateNum = GetGameStateNum(gameState);
            photonView.RPC("S_GameStateChange", RpcTarget.MasterClient, gameStateNum);
        }

        [PunRPC]
        private void S_GameStateChange(int gameStateNum)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("C_ReciveGameStateChange", RpcTarget.All, gameStateNum);
            }
        }

        [PunRPC]
        private void C_ReciveGameStateChange(int gameStateNum)
        {
            GameState gameState = GetGameState(gameStateNum);

            ChangeStateProcess(gameState);
        }

        private int GetGameStateNum(GameState gameState)
        {
            switch (gameState)
            {
                case GameStartedState: return 0;
                case PlayingGameState: return 1;
                case MeetingState: return 2;
                case VotingState: return 3;

                default:
                    Debug.Log($"Cannot find GameState for the provided number: {gameState}");
                    return 0;
            }
        }

        private GameState GetGameState(int gameStateNum)
        {
            switch (gameStateNum)
            {
                case 0: return GameManager.Instance.startedState;
                case 1: return GameManager.Instance.playingGameState;
                case 2: return GameManager.Instance.meetingState;
                case 3 : return GameManager.Instance.votingState;

                default:
                    Debug.Log($"Cannot find GameState for the provided Gamestate: {gameStateNum}");
                    return null;
            }
        }
    }
}
