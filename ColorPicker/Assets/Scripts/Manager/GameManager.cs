using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame {
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        private Dictionary<int, Player> playerDictionary = new Dictionary<int, Player>();
        private GameState gameState;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            playerDictionary = NetworkManager.Instance.S_GetPlayerDictionary();

            gameState = GameState.gameStarted;
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            S_HandleGameState();
        }

        //Only Server
        private void S_HandleGameState()
        {
            switch (gameState)
            {
                case GameState.gameStarted:
                    S_AssignPlayerClasses();
                    gameState = GameState.playingStage;
                    break;

                case GameState.playingStage: 
                    break;

                case GameState.meetingState:
                    break;

                case GameState.voteState:
                    break;

                case GameState.gameEnded:
                    break;
            }
        }


        private void S_AssignPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            S_InitalizedPlayerClasses();

            GameRuleSettings gameRules = GameDataManager.Instance.S_GetGameRules();

            List<Player> players = new List<Player>(playerDictionary.Values);

            int mafiaCount = 0;
            int tryCount = 0;

            while(mafiaCount < gameRules.mafiaAmount && tryCount < Settings.maxTryCount)
            {
                int playerIndex = Random.Range(0, players.Count);

                if (players[playerIndex].playerClassType == PlayerClassType.citizen)
                {
                    players[playerIndex].playerClassType = PlayerClassType.mafia;
                    mafiaCount++;
                }

                tryCount++;
            }

            //debug Code
            foreach(Player player in players)
            {
                Debug.Log(player.photonView.Owner.ActorNumber + " : " + player.playerClassType);
            }


        }

        private void S_InitalizedPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            foreach(Player player in playerDictionary.Values)
            {
                player.playerClassType = PlayerClassType.citizen;
            }
        }

        #region OnSever

        #endregion

        #region OnClient

        #endregion
    }
}
