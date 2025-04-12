using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame {
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    { 
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

        public void AssignPlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            InitializePlayerClasses();
            AssignMafias();

            NetworkManager.Instance.SyncPlayerData();

            SetPlayerClassAbility();

        }

        private void InitializePlayerClasses()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Dictionary<int, PlayerData> playerDataDict = NetworkManager.Instance.GetPlayerDictionary();

            foreach (int playerId in playerDataDict.Keys)
            {
                if(NetworkManager.Instance.TryGetPlayerData(playerId, out PlayerData playerData))
                {
                    playerData.playerClass = (int)PlayerClassType.citizen;
                }
                else
                {
                    Debug.LogWarning($"PlayerData for playerId {playerId} not found.");
                }
            }

        }

        private void AssignMafias()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameRuleSettings gameRules = GameDataManager.Instance.S_GetGameRules();

            Dictionary<int, PlayerData> playerDataDict = NetworkManager.Instance.GetPlayerDictionary();

            List<PlayerData> playerList = new List<PlayerData>(playerDataDict.Values);
            HelperUtilities.Shuffle(playerList);

            int mafiaCount = 0;
            foreach (PlayerData player in playerList)
            {
                if (mafiaCount >= gameRules.mafiaAmount)
                    break;

                if (NetworkManager.Instance.TryGetPlayerData(player.playerId, out PlayerData playerData))
                {
                    if (playerData.playerClass == (int)PlayerClassType.citizen)
                    {
                        Debug.Log(playerData.playerId);
                        playerData.playerClass = (int)PlayerClassType.mafia;
                        mafiaCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"PlayerData for playerId {player.playerId} not found.");
                }
            }
        }

        private void SetPlayerClassAbility()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            photonView.RPC("C_SetPlayerClasses", RpcTarget.All);
        }

        [PunRPC]
        private void C_SetPlayerClasses()
        {
            Type componentType = Type.GetType(Settings.citizenAbilityComponentName);

            switch (NetworkManager.Instance.GetPlayerDictionary()[PhotonNetwork.LocalPlayer.ActorNumber].playerClass)
            {
                case (int)PlayerClassType.mafia:
                    componentType = Type.GetType(Settings.mafiaAbilityComponentName);
                    break;
                
                default:
                    componentType = Type.GetType(Settings.citizenAbilityComponentName);
                    break;
            }

            if (NetworkManager.Instance.MyPlayer.gameObject.GetComponent(componentType) == null)
            {
                NetworkManager.Instance.MyPlayer.gameObject.AddComponent(componentType);
            }
        }

        public void ChangeMeetingStateButton()
        {
            stateMachine.ChangeState(meetingState);           
        }

    }
}
