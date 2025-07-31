using Google.Protobuf.Protocol;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MissionManager : SingletonNetworkBehaviour<MissionManager>
    {
        private Dictionary<string, PlayerMissionData> allPlayerMissions = new Dictionary<string, PlayerMissionData>();

        private List<MissionInstance> myMissions = new List<MissionInstance>(); // cache data
        private Dictionary<MiniGameType, MiniGameTag> missionObjectMap = new Dictionary<MiniGameType, MiniGameTag>();

        [SerializeField] private List<MiniGameTemplate> availableMissions;
        [SerializeField] private int missionsPerPlayer = 3; // 추후 룸세팅으로 할당 예정
        [SerializeField] private GameObject rootMissionObjects;

        protected override void Awake()
        {
            base.Awake();

            InitializedMissionObject();
        }

        public void InitializedMissionObject()
        {
            var miniGameTags = rootMissionObjects.GetComponentsInChildren<MiniGameTag>();

            foreach (MiniGameTag tag in miniGameTags)
            {
                MiniGameType type = tag.miniGameType;
                missionObjectMap[type] = tag;

                tag.gameObject.SetActive(false);
            }
        }

        public void InitializePlayerMissions(List<PublicPlayerData> playerDatas)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            allPlayerMissions.Clear();

            foreach (PublicPlayerData playerData in playerDatas)
            {
                string uid = playerData.googleUID;
                int actorId = playerData.currentActorId;

                if (!GameDataManager.Instance.GetInGameData(uid).isAlive) continue;

                var assignedMissions = GetUniqueMissionsForPlayer(uid, missionsPerPlayer);

                var missionData = new PlayerMissionData
                {
                    playerUID = uid,
                    missionList = assignedMissions
                };

                allPlayerMissions.Add(playerData.googleUID, missionData);

                Photon.Realtime.Player target = PhotonNetwork.CurrentRoom.Players[playerData.currentActorId];

                photonView.RPC(nameof(RPC_AssignMissionList), target,
                    JsonUtility.ToJson(missionData));
            }

            GameDataManager.Instance.SetMissionBackup(allPlayerMissions);
        }

        private List<MissionInstance> GetUniqueMissionsForPlayer(string playerUID, int count)
        {
            if (availableMissions.Count < count)
            {
                Debug.Log("할당가능한 미션 부족");
                return null;
            }

            return availableMissions
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(count)
                .Select(template => new MissionInstance
                {
                    missionId = $"{template.miniGameName}",
                    missionType = template.miniGameType,
                    isCompleted = false
                })
                .ToList();
        }

        [PunRPC]
        private void RPC_AssignMissionList(string json)
        {
            PlayerMissionData playerMissionData = JsonUtility.FromJson<PlayerMissionData>(json);

            myMissions = playerMissionData.missionList;

            ApplyMyMissionList();
        }

        private void ApplyMyMissionList()
        {
            foreach (MiniGameTag tag in missionObjectMap.Values)
            {
                bool hasMatchingMission = myMissions.Exists(mission => mission.missionType == tag.miniGameType);

                if (hasMatchingMission)
                {
                    tag.gameObject.SetActive(true);
                }
                else
                {
                    tag.gameObject.gameObject.SetActive(false);
                }
            }
        }

        public void RequestMissionComplete(int playerId, int miniGameTypeInt)
        {
            photonView.RPC(nameof(RPC_UpdateMissionStatus), RpcTarget.MasterClient, playerId, miniGameTypeInt);
        }


        [PunRPC]
        public void RPC_UpdateMissionStatus(int actorId, int miniGameTypeInt)
        {
            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorId, out PublicPlayerData data))
            {
                UpdateMissionStatus(data.googleUID, miniGameTypeInt);
            }
            else
            {
                Debug.LogWarning($"[MissionManager] ActorID {actorId} 에 해당하는 PublicPlayerData 를 찾을 수 없음");
            }
        }

        public void UpdateMissionStatus(string playerId, int miniGameTypeInt)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            MiniGameType miniGameType = (MiniGameType)miniGameTypeInt;

            if (!allPlayerMissions.TryGetValue(playerId, out var playerData)) return;
            
            var mission = playerData.missionList.FirstOrDefault(m => m.missionType == miniGameType && !m.isCompleted);

            if (mission == null)
            {
                Debug.LogWarning($"[MissionManager] {playerId}의 {miniGameType} 미션을 찾을 수 없거나 이미 완료됨.");
                return;
            }

            mission.isCompleted = true;

            GameDataManager.Instance.TryGetPublicPlayerData(playerId, out PublicPlayerData data);

            Photon.Realtime.Player target = PhotonNetwork.CurrentRoom.Players[data.currentActorId];

            string json = JsonUtility.ToJson(allPlayerMissions[playerId]);

            photonView.RPC(nameof(Rpc_UpdateMissionStatus), target, json);

            GiveMissionReward(playerId, target);

            float percent = GetMissionCompletePercent();

            photonView.RPC(nameof(Rpc_UpdateMissionStatusBar), RpcTarget.All, percent);

        }

        [PunRPC]
        public void Rpc_UpdateMissionStatus(string json)
        {
           PlayerMissionData mission = JsonUtility.FromJson<PlayerMissionData>(json);

           foreach(var missionInstance in mission.missionList)
            {
                if (missionInstance.isCompleted) 
                    missionObjectMap[missionInstance.missionType].gameObject.SetActive(false);
                else
                    missionObjectMap[missionInstance.missionType].gameObject.SetActive(true);

            }

           UIManager.Instance.UpdateMissionStatusUI(mission);
        }

        private float GetMissionCompletePercent()
        {
            var allMissions = allPlayerMissions.Values.SelectMany(data => data.missionList);
            int totalCount = allMissions.Count();

            if (totalCount == 0) return 0f;

            int successCount = allMissions.Count(m => m.isCompleted);
            return (float)successCount / totalCount;
        }

        [PunRPC]
        public void Rpc_UpdateMissionStatusBar(float percent)
        {
            UIManager.Instance.UpdateMissionStatusBarUI(percent);
        }

        private void GiveMissionReward(string playerUID, Photon.Realtime.Player target)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            bool success = GameDataManager.Instance.TryAddMissionReward(playerUID);

            int stickerAmout = GameDataManager.Instance.GetInGameData(playerUID).stickerCount;

            if (success)
            {
                photonView.RPC(nameof(Rpc_ReceiveReward), target, stickerAmout);
            }
        }

        [PunRPC]
        private void Rpc_ReceiveReward(int stickerAmout)
        {
            //TODO : UI 설정
        }
    }
}
