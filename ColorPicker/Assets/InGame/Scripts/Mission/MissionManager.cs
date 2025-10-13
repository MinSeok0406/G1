using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// Host-authoritative Mission Manager
    /// 미션 할당, 완료 처리, 진행률 관리를 담당
    /// </summary>
    public sealed class MissionManager : SingletonNetworkBehaviour<MissionManager>
    {
        #region Events
        public event Action<float> OnMissionProgressChanged;
        public event Action<string, MiniGameType> OnMissionCompleted;
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private RectTransform missionTaskContainerRect;
        [SerializeField] private GameObject missionTaskInfoContent;
        [SerializeField] private GameObject rootMissionObjects;

        [Header("Configuration")]
        [SerializeField] private List<MiniGameTemplate> availableMissions;
        [SerializeField, Range(1, 10)] private int missionsPerPlayer = 3;
        [SerializeField] private bool deactivateUnassignedOnInit = true;
        #endregion

        #region Private Fields
        // Host-only: 모든 플레이어의 미션 데이터
        private readonly Dictionary<string, PlayerMissionData> _allPlayerMissions = new();
        
        // Client-side: 내 미션 캐시
        private readonly List<MissionInstance> _myMissions = new();
        
        // 씬 내 미션 오브젝트 매핑
        private readonly Dictionary<MiniGameType, MiniGameTag> _missionObjectMap = new();
        
        // UI 매핑
        private readonly Dictionary<MiniGameType, MissionTaskItem> _missionUIMap = new();

        // 의존성
        private IMissionAssigner _missionAssigner;
        private IMissionValidator _missionValidator;
        private IMissionRewardProvider _rewardProvider;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            InitializeDependencies();
            InitializeMissionObjects();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            
            if (_myMissions.Count > 0)
                ApplyMyMissionList();
        }

        private void OnDestroy()
        {
            CleanupUI();
        }
        #endregion

        #region Initialization
        private void InitializeDependencies()
        {
            _missionAssigner = new UniqueMissionAssigner(availableMissions);
            _missionValidator = new MissionValidator();
            _rewardProvider = new MissionRewardProvider();
        }

        /// <summary>
        /// 씬 내 미니게임 태그 수집 (비활성 오브젝트 포함)
        /// </summary>
        public void InitializeMissionObjects()
        {
            _missionObjectMap.Clear();

            if (rootMissionObjects == null)
            {
                Debug.LogWarning("[MissionManager] rootMissionObjects is null.");
                return;
            }

            var miniGameTags = rootMissionObjects.GetComponentsInChildren<MiniGameTag>(true);
            
            foreach (var tag in miniGameTags)
            {
                if (tag == null || _missionObjectMap.ContainsKey(tag.miniGameType))
                    continue;

                _missionObjectMap[tag.miniGameType] = tag;

                if (deactivateUnassignedOnInit)
                    tag.gameObject.SetActive(false);
            }

            Debug.Log($"[MissionManager] Initialized {_missionObjectMap.Count} mission objects.");
        }
        #endregion

        #region Public API - Mission Assignment
        /// <summary>
        /// [Host Only] 라운드 시작 시 생존한 플레이어들에게 미션 재할당
        /// </summary>
        public void InitializeRoundMissions(List<PublicPlayerData> playerDatas)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("[MissionManager] InitializeRoundMissions can only be called by master client.");
                return;
            }

            if (playerDatas == null || playerDatas.Count == 0)
            {
                Debug.LogWarning("[MissionManager] No players to assign missions.");
                return;
            }

            _allPlayerMissions.Clear();
            int assignedCount = 0;

            foreach (var playerData in playerDatas)
            {
                if (TryAssignMissionsToPlayer(playerData))
                    assignedCount++;
            }

            SaveMissionBackup();
            
            Debug.Log($"[MissionManager] Round missions initialized for {assignedCount}/{playerDatas.Count} players.");
        }

        /// <summary>
        /// [Host Only] 개별 플레이어에게 미션 할당 시도
        /// </summary>
        private bool TryAssignMissionsToPlayer(PublicPlayerData playerData)
        {
            if (playerData == null || string.IsNullOrEmpty(playerData.googleUID))
                return false;

            // 사망자는 미션 할당 제외
            var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
            if (inGameData == null || !inGameData.isAlive)
            {
                Debug.Log($"[MissionManager] Skipping mission assignment for dead player: {playerData.googleUID}");
                return false;
            }

            // 미션 할당
            var assignedMissions = _missionAssigner.AssignMissions(playerData.googleUID, missionsPerPlayer);
            if (assignedMissions == null || assignedMissions.Count == 0)
            {
                Debug.LogWarning($"[MissionManager] Failed to assign missions for {playerData.googleUID}");
                return false;
            }

            var missionData = new PlayerMissionData
            {
                playerUID = playerData.googleUID,
                missionList = assignedMissions
            };

            _allPlayerMissions[playerData.googleUID] = missionData;

            // RPC로 해당 플레이어에게 미션 전송
            SendMissionsToPlayer(playerData.currentActorId, missionData);

            return true;
        }

        /// <summary>
        /// 특정 플레이어에게 미션 데이터 전송
        /// </summary>
        private void SendMissionsToPlayer(int actorId, PlayerMissionData missionData)
        {
            if (!PhotonNetwork.CurrentRoom.Players.TryGetValue(actorId, out Photon.Realtime.Player targetPlayer))
            {
                Debug.LogWarning($"[MissionManager] Target actor {actorId} not found in room.");
                return;
            }

            string json = JsonUtility.ToJson(missionData);
            photonView.RPC(nameof(RPC_AssignMissionList), targetPlayer, json);
        }
        #endregion

        #region RPC - Mission Assignment
        [PunRPC]
        private void RPC_AssignMissionList(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[MissionManager] Received empty mission data.");
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<PlayerMissionData>(json);
                if (data?.missionList == null)
                {
                    Debug.LogWarning("[MissionManager] Invalid mission data received.");
                    return;
                }

                _myMissions.Clear();
                _myMissions.AddRange(data.missionList);

                ApplyMyMissionList();
                
                Debug.Log($"[MissionManager] Received {_myMissions.Count} missions.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MissionManager] Failed to parse mission data: {ex.Message}");
            }
        }

        /// <summary>
        /// 내 미션 목록을 씬에 적용 (오브젝트 활성화 + UI 생성)
        /// </summary>
        private void ApplyMyMissionList()
        {
            CleanupUI();

            var activeMissionTypes = PoolUtility.GetHashSet<MiniGameType>();

            try
            {
                // 내 미션 타입 수집
                foreach (var mission in _myMissions)
                {
                    activeMissionTypes.Add(mission.missionType);
                }

                // 씬 오브젝트 활성화 및 UI 생성
                foreach (var kvp in _missionObjectMap)
                {
                    var missionType = kvp.Key;
                    var missionTag = kvp.Value;

                    if (missionTag == null) continue;

                    bool isMyMission = activeMissionTypes.Contains(missionType);
                    missionTag.gameObject.SetActive(isMyMission);

                    if (isMyMission)
                    {
                        CreateMissionUI(missionType);
                    }
                }
            }
            finally
            {
                PoolUtility.ReturnHashSet(activeMissionTypes);
            }
        }

        /// <summary>
        /// 특정 미션 타입의 UI 생성
        /// </summary>
        private void CreateMissionUI(MiniGameType missionType)
        {
            if (missionTaskInfoContent == null || missionTaskContainerRect == null)
            {
                Debug.LogWarning("[MissionManager] UI prefab or container is missing.");
                return;
            }

            if (!TryGetMissionInfo(missionType, out string missionInfo))
            {
                Debug.LogWarning($"[MissionManager] No mission info found for {missionType}");
                return;
            }

            var uiObject = Instantiate(missionTaskInfoContent, missionTaskContainerRect);
            var taskItem = uiObject.GetComponent<MissionTaskItem>();
            
            if (taskItem == null)
                taskItem = uiObject.AddComponent<MissionTaskItem>();

            taskItem.Init(missionType, missionInfo);
            _missionUIMap[missionType] = taskItem;
        }
        #endregion

        #region Public API - Mission Completion
        /// <summary>
        /// [Client] 미션 완료 요청 (호스트로 전송)
        /// </summary>
        public void RequestMissionComplete(int playerId, MiniGameType miniGameType)
        {
            photonView.RPC(nameof(RPC_CompleteMission), RpcTarget.MasterClient, playerId, (int)miniGameType);
        }
        #endregion

        #region RPC - Mission Completion
        [PunRPC]
        private void RPC_CompleteMission(int actorId, int miniGameTypeInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 스푸핑 방지: 발신자 검증
            if (info.Sender == null || info.Sender.ActorNumber != actorId)
            {
                Debug.LogWarning($"[MissionManager] Spoofed completion request. Sender={info.Sender?.ActorNumber}, Arg={actorId}");
                return;
            }

            if (!GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorId, out PublicPlayerData playerData))
            {
                Debug.LogWarning($"[MissionManager] Player data not found for actor {actorId}");
                return;
            }

            ProcessMissionCompletion(playerData.googleUID, (MiniGameType)miniGameTypeInt, actorId);
        }

        /// <summary>
        /// [Host Only] 미션 완료 처리
        /// </summary>
        private void ProcessMissionCompletion(string playerUID, MiniGameType missionType, int actorId)
        {
            if (string.IsNullOrEmpty(playerUID)) return;

            if (!_allPlayerMissions.TryGetValue(playerUID, out var playerMission) || 
                playerMission?.missionList == null)
            {
                Debug.LogWarning($"[MissionManager] No missions found for player {playerUID}");
                return;
            }

            // 미션 유효성 검증
            if (!_missionValidator.TryCompleteMission(playerMission.missionList, missionType, out int completedIndex))
            {
                Debug.LogWarning($"[MissionManager] Invalid mission completion: {playerUID}, {missionType}");
                return;
            }

            // 미션 완료 상태 업데이트
            var mission = playerMission.missionList[completedIndex];
            mission.isCompleted = true;
            playerMission.missionList[completedIndex] = mission;

            // 클라이언트에 완료 상태 전송
            NotifyMissionCompletion(playerUID, actorId, playerMission);

            // 진행률 업데이트
            BroadcastMissionProgress();

            // 보상 지급
            GiveMissionReward(playerUID, actorId);

            // 백업 저장
            SaveMissionBackup();

            OnMissionCompleted?.Invoke(playerUID, missionType);
            
            Debug.Log($"[MissionManager] Mission completed: {playerUID}, {missionType}");
        }

        /// <summary>
        /// 클라이언트에 미션 완료 알림
        /// </summary>
        private void NotifyMissionCompletion(string playerUID, int actorId, PlayerMissionData missionData)
        {
            if (!PhotonNetwork.CurrentRoom.Players.TryGetValue(actorId, out Photon.Realtime.Player targetPlayer))
            {
                Debug.LogWarning($"[MissionManager] Target player not found: {actorId}");
                return;
            }

            string json = JsonUtility.ToJson(missionData);
            photonView.RPC(nameof(RPC_UpdateMissionStatus), targetPlayer, json);
        }

        [PunRPC]
        private void RPC_UpdateMissionStatus(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var missionData = JsonUtility.FromJson<PlayerMissionData>(json);
                if (missionData?.missionList == null) return;

                UpdateLocalMissionStatus(missionData.missionList);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MissionManager] Failed to update mission status: {ex.Message}");
            }
        }

        /// <summary>
        /// 로컬 미션 상태 업데이트 (오브젝트 비활성화 + UI 업데이트)
        /// </summary>
        private void UpdateLocalMissionStatus(List<MissionInstance> missions)
        {
            foreach (var mission in missions)
            {
                if (!_missionObjectMap.TryGetValue(mission.missionType, out var missionTag) || 
                    missionTag == null)
                    continue;

                // 완료된 미션 오브젝트 비활성화
                missionTag.gameObject.SetActive(!mission.isCompleted);

                // UI 업데이트
                if (_missionUIMap.TryGetValue(mission.missionType, out var taskItem) && 
                    taskItem != null)
                {
                    taskItem.SetCompleted(mission.isCompleted);
                }
            }
        }
        #endregion

        #region Mission Progress
        /// <summary>
        /// [Host Only] 전체 미션 진행률 브로드캐스트
        /// </summary>
        private void BroadcastMissionProgress()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float progress = CalculateMissionProgress();
            photonView.RPC(nameof(RPC_UpdateMissionProgress), RpcTarget.All, progress);
        }

        /// <summary>
        /// 전체 미션 진행률 계산 (0.0 ~ 1.0)
        /// </summary>
        public float CalculateMissionProgress()
        {
            int totalMissions = 0;
            int completedMissions = 0;

            foreach (var kvp in _allPlayerMissions)
            {
                var missions = kvp.Value?.missionList;
                if (missions == null) continue;

                foreach (var mission in missions)
                {
                    totalMissions++;
                    if (mission.isCompleted)
                        completedMissions++;
                }
            }

            return totalMissions > 0 ? (float)completedMissions / totalMissions : 0f;
        }

        /// <summary>
        /// 전체 미션 진행률 퍼센트 반환 (0 ~ 100)
        /// </summary>
        public float GetMissionProgressPercent()
        {
            return CalculateMissionProgress() * 100f;
        }

        [PunRPC]
        private void RPC_UpdateMissionProgress(float progress)
        {
            UIManager.Instance?.UpdateMissionStatusBarUI(progress);
            OnMissionProgressChanged?.Invoke(progress);
        }
        #endregion

        #region Rewards
        /// <summary>
        /// [Host Only] 미션 보상 지급
        /// </summary>
        private void GiveMissionReward(string playerUID, int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (string.IsNullOrEmpty(playerUID)) return;

            var reward = _rewardProvider.GetMissionReward();
            
            if (!GameDataManager.Instance.TryAddMissionReward(playerUID))
            {
                Debug.LogWarning($"[MissionManager] Failed to add reward for {playerUID}");
                return;
            }

            var inGameData = GameDataManager.Instance.GetInGameData(playerUID);
            int coinAmount = inGameData?.coinCount ?? 0;

            if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorId, out Photon.Realtime.Player targetPlayer))
            {
                photonView.RPC(nameof(RPC_ReceiveReward), targetPlayer, coinAmount);
            }
        }

        [PunRPC]
        private void RPC_ReceiveReward(int coinAmount)
        {
            UIManager.Instance?.UpdateCoinInfo(coinAmount);
        }
        #endregion

        #region UI Management
        /// <summary>
        /// 모든 미션 UI 제거
        /// </summary>
        public void CleanupUI()
        {
            foreach (var kvp in _missionUIMap)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                    Destroy(kvp.Value.gameObject);
            }

            _missionUIMap.Clear();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 미션 정보 조회
        /// </summary>
        private bool TryGetMissionInfo(MiniGameType type, out string info)
        {
            info = null;
            
            if (availableMissions == null) return false;

            foreach (var template in availableMissions)
            {
                if (template.miniGameType == type)
                {
                    info = template.missionInfo;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 미션 데이터 백업 저장
        /// </summary>
        private void SaveMissionBackup()
        {
            GameDataManager.Instance?.SetMissionBackup(_allPlayerMissions);
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugPrintMissions()
        {
            Debug.Log($"===== Mission Status (Total Players: {_allPlayerMissions.Count}) =====");
            
            foreach (var kvp in _allPlayerMissions)
            {
                var missions = kvp.Value?.missionList;
                if (missions == null) continue;

                int completed = 0;
                foreach (var mission in missions)
                {
                    if (mission.isCompleted) completed++;
                }

                Debug.Log($"Player {kvp.Key}: {completed}/{missions.Count} missions completed");
            }

            Debug.Log($"Total Progress: {GetMissionProgressPercent():F1}%");
        }
        #endregion
    }
}