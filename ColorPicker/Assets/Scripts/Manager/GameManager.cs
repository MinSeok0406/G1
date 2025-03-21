using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame {
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        private Dictionary<int, PlayerData> playerDictionary = new Dictionary<int, PlayerData>();
        private GameState gameState;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            playerDictionary = NetworkManager.Instance.GetPlayerDictionary();

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

            S_SyncGameState(gameState);
        }

        private void S_SyncGameState(GameState gameState)
        {
            photonView.RPC("C_SyncGameState", RpcTarget.All, gameState);
        }

        [PunRPC]
        private void C_SyncGameState(GameState gameState)
        {
            this.gameState = gameState;
        }

        private void S_AssignPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            S_InitalizedPlayerClasses();

            GameRuleSettings gameRules = GameDataManager.Instance.S_GetGameRules();

            List<PlayerData> playerDatas = new List<PlayerData>(playerDictionary.Values);

            int mafiaCount = 0;
            int tryCount = 0;

            while (mafiaCount < gameRules.mafiaAmount && tryCount < Settings.maxTryCount)
            {
                int playerIndex = UnityEngine.Random.Range(0, playerDatas.Count);

                if (playerDatas[playerIndex].player.playerClassType == PlayerClassType.citizen)
                {
                    int playerId = playerDatas[playerIndex].playerId;

                    playerDictionary[playerId].player.playerClassType = PlayerClassType.mafia;

                    Debug.Log(playerDictionary[playerId].player.playerClassType);

                    Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players[playerId];

                    photonView.RPC("C_SetPlayerClasses", targetPlayer, Settings.mafiaAbilityComponentName);
                    
                    mafiaCount++;
                }

                tryCount++;
            }

            S_SetCitizenPlayerClasses();


            //debug Code
            foreach (PlayerData playerData in playerDictionary.Values)
            {
                Debug.Log(playerData.player.photonView.Owner.ActorNumber + " : " + playerData.player.playerClassType);
            }

        }

        private void S_InitalizedPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (PlayerData playerData in playerDictionary.Values)
            {
                playerData.player.playerClassType = PlayerClassType.citizen;
            }
        }

        private void S_SetCitizenPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (PlayerData playerData in playerDictionary.Values)
            {
                if(playerData.player.playerClassType == PlayerClassType.citizen)
                {
                    Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players[playerData.playerId];

                    photonView.RPC("C_SetPlayerClasses", targetPlayer, Settings.citizenAbilityComponentName);
                }
            }
        }

        [PunRPC]
        private void C_SetPlayerClasses(string className)
        {
            Type componentType = Type.GetType(className);

            if(componentType == null)
            {
                Debug.LogError($"Class '{className}' not found.");                
            }

            if (NetworkManager.Instance.MyPlayer.gameObject.GetComponent(componentType) == null)
            {
                NetworkManager.Instance.MyPlayer.gameObject.AddComponent(componentType);
            }
            else
            {
                Debug.LogError($"{PhotonNetwork.LocalPlayer.ActorNumber}Class '{className}' already exists");
            }
            
        }

    }
}
