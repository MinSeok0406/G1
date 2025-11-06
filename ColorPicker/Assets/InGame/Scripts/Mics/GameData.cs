using System;
using System.Collections.Generic;

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
        public bool isAlive = true; // 생존 여부
        public bool hasVoted = false; // 투표 여부
        public int coinCount = 0;
        public int paintCount = 0;
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
        // ===== 플레이어 설정 =====
        public int minPlayers = 4;              // 최소 인원수
        public int maxPlayers = 10;             // 최대 인원수
        public int mafiaAmount = 2;             // 마피아 수
        public int detectiveAmount = 1;         // 탐정 수

        // ===== 미팅 설정 =====
        public int maxMeetingTimeSec = 120;     // 회의 시간 (초)
        public float emergencyMeetingCooldown = 30f; // 긴급소집 쿨다운 (초)

        // ===== 미션 설정 =====
        public int missionsPerPlayer = 3;       // 한 라운드에 할당받는 미션 개수
        public float minMissionProgressForEmergency = 0.3f; // 긴급소집 최소 미션 진행도 (0.0 ~ 1.0)

        // ===== 페인트 설정 =====
        public int paintCost = 10;              // 페인트 가격 (코인)
        public int maxPaintPerRound = 5;        // 한 라운드에 칠할 수 있는 페인트 개수
        public int maxPaintExchangePerRound = 3; // 한 라운드에 교환할 수 있는 페인트 개수

        // ===== 쿨타임 설정 =====
        public float mafiaKillCooldown = 30f;   // 마피아 킬 쿨타임 (초)
        public float mafiaInitialCooldownPercent = 0.5f; // 마피아 라운드 시작 시 쿨타임 비율 (0.0 ~ 1.0)

        /// <summary>
        /// 기본 권장 설정으로 초기화
        /// </summary>
        public void SetDefaultSettings()
        {
            minPlayers = 4;
            maxPlayers = 10;
            mafiaAmount = 2;
            detectiveAmount = 1;
            maxMeetingTimeSec = 120;
            emergencyMeetingCooldown = 30f;
            missionsPerPlayer = 3;
            minMissionProgressForEmergency = 0.3f;
            paintCost = 10;
            maxPaintPerRound = 5;
            maxPaintExchangePerRound = 3;
            mafiaKillCooldown = 30f;
            mafiaInitialCooldownPercent = 0.5f;
        }
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

    [Serializable]
    public class MissionInstance
    {
        public string missionId;  
        public MiniGameType missionType;    
        public bool isCompleted;           
    }

    [Serializable]
    public class PlayerMissionData
    {
        public string playerUID; 
        public List<MissionInstance> missionList = new List<MissionInstance>();
    }
}