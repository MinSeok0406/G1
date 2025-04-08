using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame {
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        private Dictionary<int, PlayerData> playerDictionary = new Dictionary<int, PlayerData>();

        #region GameState
        public GameStateMachine stateMachine { get; private set; }

        public GameStartedState startedState { get; private set; }
        public PlayingGameState playingGameState { get; private set; }
        public MeetingState meetingState { get; private set; }
        public VotingState votingState { get; private set; }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            stateMachine = GetComponent<GameStateMachine>();

            startedState = new GameStartedState(stateMachine);
            playingGameState = new PlayingGameState(stateMachine);
            meetingState = new MeetingState(stateMachine);
            votingState = new VotingState(stateMachine);
        }

        private void Start()
        {
            stateMachine.Initialize(startedState);
        }

        private void Update()
        {
            stateMachine.currentState.Update();
        }

        public void InitializedGameManager()
        {
            playerDictionary = NetworkManager.Instance.GetPlayerDictionary();
        }

        
        public void S_AssignPlayerClasses()
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

        public void C_SendPlayerState(int playerId)
        {
            photonView.RPC("S_SendPlayerStateUpdate", RpcTarget.MasterClient, playerId);
        }

        [PunRPC]
        public void S_SendPlayerStateUpdate(int playerId)
        {
            photonView.RPC("C_RecivePlayerState", RpcTarget.All, playerId);
        }

        [PunRPC]
        private void C_RecivePlayerState(int playerId)
        {
            playerDictionary[playerId].isAlive = false;

            Debug.Log(playerId + " : " + playerDictionary[playerId].isAlive); // 추후 onKillEvent로 추가 예정 
        }


        public void ChangeMeetingStateButton()
        {
            stateMachine.ChangeState(meetingState);           
        }

    }
}
