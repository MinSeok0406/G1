using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

            HandleGameState();
        }

        //Only Server
        private void HandleGameState()
        {
            switch (gameState)
            {
                case GameState.gameStarted:
                    AssignPlayerClasses();
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

        private void AssignPlayerClasses()
        {
           


        }

        private void InitalizedPlayerClasses()
        {
            foreach(Player player in playerDictionary.Values)
            {
                
            }
        }

        #region OnSever

        #endregion

        #region OnClient

        #endregion
    }
}
