using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ColorPicker.InGame
{
    [System.Serializable]
    public class LobbyPlayerData
    {
        public LobbyPlayerStaticData staticData;
        public LobbyPlayerDynamicData dynamicData;
    }

    [System.Serializable]
    public class LobbyPlayerStaticData
    {
        public string googleUID;
        public string nickname;
        public int actorId;
        public int viewId;
    }

    [System.Serializable]
    public class LobbyPlayerDynamicData
    {
        public float posX;
        public float posY;
        public bool isReady;
        public PlayerCustomizationData playerCustomizationData;
    }

    [System.Serializable]
    public class LobbyPlayerDataListWrapper
    {
        public List<LobbyPlayerData> lobbyPlayerDatas;
    }

    [System.Serializable]
    public class PublicPlayerData
    {
        public string googleUID;
        public string nickname;
        public int currentActorId;
        public PlayerCustomizationData customizationData; 
    }

    [System.Serializable]
    public class PrivatePlayerData
    {
        public string googleUID;
        public int currentActorId;
        public int classType; // 직업 ID
        public int identityColorId; // 인게임 컬러(역할)
    }

    [System.Serializable]
    public class InGameData
    {
        public bool isAlive = true;            // 생존 여부
        public bool hasVoted = false;          // 투표 여부
    }

    [System.Serializable]
    public class InGameDataEntry
    {
        public string uid;
        public InGameData data;
    }

    [System.Serializable]
    public class GameRuleSettings
    {
        public int mafiaAmount;
        public int detectiveAmount;

        public int maxMeetingTimeSec;
        public int maxPlayers;
    }

    [System.Serializable]
    public class PlayerCustomizationData
    {
        public int customColorId;   // 사용자 지정 외형 색상 ID 
        public int hatId;           // 모자 ID
    }

    [System.Serializable]
    public class ViewIDEntry
    {
        public string googleUID;
        public int viewID;
    }
}