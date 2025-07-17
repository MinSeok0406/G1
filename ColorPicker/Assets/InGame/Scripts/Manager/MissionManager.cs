using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MissionManager : SingletonNetworkBehaviour<MissionManager>
    {
        private Dictionary<string, PlayerMissionData> allPlayerMissions = new Dictionary<string, PlayerMissionData>();

        private List<MissionInstance> myMissions = new List<MissionInstance>(); // cache data

        [SerializeField] private List<MiniGameTemplate> availableMissions;
        [SerializeField] private int missionsPerPlayer = 3; // 추후 룸세팅으로 할당 예정

        protected override void Awake()
        {
            base.Awake();   
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

                photonView.RPC(nameof(RPC_AssignMissionList),target,
                    JsonUtility.ToJson(missionData));
            }

            GameDataManager.Instance.SetMissionBackup(allPlayerMissions);
        }

        private List<MissionInstance> GetUniqueMissionsForPlayer(string playerUID, int count)
        {
            if(allPlayerMissions.Count < count)
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

        }

        public void UpdateMissionStatus(string playerId, MiniGameType miniGameType)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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

            float percent = GetMissionCompletePercent();

            photonView.RPC(nameof(Rpc_UpdateMissionStatusBar), RpcTarget.All);

        }

        [PunRPC]
        public void Rpc_UpdateMissionStatus(string json)
        {
           PlayerMissionData mission = JsonUtility.FromJson<PlayerMissionData>(json);

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
    }
}
