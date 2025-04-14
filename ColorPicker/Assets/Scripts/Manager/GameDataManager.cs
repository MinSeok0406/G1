using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameDataManager : SingletonNetworkBehaviour<GameDataManager>
    {
        private GameRuleSettings gameRules;
        private Dictionary<int, InGameData> inGamePlayerDatas = new Dictionary<int, InGameData>();

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializedGameRule();
        }

        private void InitializedGameRule()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            gameRules = new GameRuleSettings();

            gameRules.SetRuleSettingRecomend();
        }

        // 필요한 데이터 원본 반환
        public bool TryGetInGameData(int playerId,out InGameData inGamePlayerData)
        {
            return this.inGamePlayerDatas.TryGetValue(playerId, out inGamePlayerData);
        }

        // 데이터 원본 최신화
        public void UpdateInGameData(int playerId, InGameData data)
        {
            inGamePlayerDatas[playerId] = data; // 덮어씌워 진 Data 객체는 가비지 컬렉터에 의해 제거 됨.
        }

        // 생존 여부만 판단 (killEvent에서 사용)
        public bool TryUpdateAliveState(int playerId, bool isAlive)
        {
            if (!inGamePlayerDatas.TryGetValue(playerId, out var data)) return false;

            data.alive = isAlive;
            return true;
        }

        public Dictionary<int, InGameData> GetInGameDataDictionary()
        {
            return inGamePlayerDatas;
        }

        public GameRuleSettings S_GetGameRules()
        {
            return gameRules;
        }
    }

    public class InGameData
    {
        public int playerId;
        public bool alive = true;
        public int colorType;
    }
}
