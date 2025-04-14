using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
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

            AssignColor();

            SetPlayerClassAbility();

            NetworkManager.Instance.SyncPlayerData();
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
                    Debug.Log($"PlayerData for playerId {playerId} not found.");
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

            foreach (PlayerData player in NetworkManager.Instance.GetPlayerDictionary().Values)
            {
                int targetPlayerId = player.playerId;
                int playerClass = player.playerClass;

                Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(targetPlayerId);

                if (targetPlayer != null)
                {
                    photonView.RPC("PRC_SetPlayerClass", targetPlayer, playerClass);
                }
                else
                {
                    Debug.Log($"{targetPlayerId} class not define");
                }
            }
        }

        [PunRPC]
        private void PRC_SetPlayerClass(int playerClass)
        {
            Type componentType = GetAbilityComponentType(playerClass);

            if (componentType == null)
            {
                Debug.LogError($"Player class {playerClass} not found");
                return;
            }

            GameObject myPlayerObj = NetworkManager.Instance.MyPlayer.gameObject;

            if (myPlayerObj.GetComponent(componentType) == null)
            {
                myPlayerObj.AddComponent(componentType);
            }
            else
            {
                Debug.Log($"{componentType.Name} already added");
            }
        }

        private Type GetAbilityComponentType(int playerClass)
        {
            string componentName;

            switch ((PlayerClassType)playerClass)
            {
                case PlayerClassType.mafia:
                    componentName = Settings.mafiaAbilityComponentName;
                    break;

                case PlayerClassType.citizen:
                    componentName = Settings.citizenAbilityComponentName;
                    break;

                default:
                    Debug.Log($"Validation Error : {playerClass}");
                    return null;
            }

            Type componentType = Type.GetType(componentName);

            if (componentType == null)
            {
                Debug.LogError($"Player component {componentName} not found");
            }

            return componentType;
        }

        private void AssignColor()
        {

            if (!PhotonNetwork.IsMasterClient) return;

            var playerDict = NetworkManager.Instance.GetPlayerDictionary();

            // 사용 가능한 색상 리스트 생성. 형변환 후 검정, 흰색이아닌 색상을 리스트에 저장
            List<ColorType> availableColors = Enum.GetValues(typeof(ColorType))
                .Cast<ColorType>()
                .Where(color => color != ColorType.Black && color != ColorType.White)
                .ToList();

            HelperUtilities.Shuffle(availableColors); // 무작위 순서로 셔플

            int colorIndex = 0;

            foreach (int playerId in playerDict.Keys)
            {
                InGameData gameData = new InGameData(); 

                gameData.playerId = playerId;

                var playerData = playerDict[playerId];

                // 마피아는 검정색
                if (playerData.playerClass == (int)PlayerClassType.mafia)
                {
                    gameData.colorType = (int)ColorType.Black;
                }
                else
                {
                    gameData.colorType = (int)availableColors[colorIndex];
                    colorIndex++;
                }

                GameDataManager.Instance.UpdateInGameData(playerId, gameData); 
            }
        }


        public void ChangeMeetingStateButton()
        {
            stateMachine.ChangeState(meetingState);           
        }
    }
}
