using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// Host-authoritative Mission Manager
    /// 기존 공개된 메서드/RPC 이름 유지(호환성 보장)
    /// </summary>
    public sealed class MissionManager : SingletonNetworkBehaviour<MissionManager>
    {
        // host only: UID -> PlayerMissionData
        private readonly Dictionary<string, PlayerMissionData> allPlayerMissions = new();

        // local cache (내 미션만)
        private readonly List<MissionInstance> myMissions = new();

        // 미션 타입 -> 씬 내 태그(오브젝트)
        private readonly Dictionary<MiniGameType, MiniGameTag> missionObjectMap = new();

        [Header("Ref")]
        [SerializeField] private RectTransform missionTaskContainerRect; // 미션 UI 컨텐츠(스크롤뷰)
        [SerializeField] private GameObject missionTaskInfoContent; // 미션 UI 컨텐츠(스크롤뷰)

        [Header("Config")]
        [SerializeField] private List<MiniGameTemplate> availableMissions;
        [SerializeField, Min(1)] private int missionsPerPlayer = 3;
        [SerializeField] private GameObject rootMissionObjects;
        [SerializeField] private bool deactivateUnassignedOnInit = true; // 내 미션 아닌 태그 비활성화

        private readonly Dictionary<MiniGameType, MissionTaskItem> missionUIMap = new();

        protected override void Awake()
        {
            base.Awake();
            InitializedMissionObject();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            // 태그가 재활성화되었을 때 내 캐시를 다시 반영
            if (myMissions.Count > 0)
                ApplyMyMissionList();
        }

        /// <summary>
        /// 씬 내 미니게임 태그 재수집(비활성 포함)
        /// </summary>
        public void InitializedMissionObject()
        {
            missionObjectMap.Clear();

            if (!rootMissionObjects)
            {
                Debug.LogWarning("[Mission] rootMissionObjects is null.");
                return;
            }

            var miniGameTags = rootMissionObjects.GetComponentsInChildren<MiniGameTag>(true);
            for (int i = 0; i < miniGameTags.Length; i++)
            {
                var tag = miniGameTags[i];
                if (!missionObjectMap.ContainsKey(tag.miniGameType))
                    missionObjectMap.Add(tag.miniGameType, tag);

                if (deactivateUnassignedOnInit)
                    tag.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 호스트: 각 플레이어에게 유니크 미션을 할당하고 전송
        /// </summary>
        public void InitializePlayerMissions(List<PublicPlayerData> playerDatas)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (playerDatas == null || playerDatas.Count == 0) return;

            allPlayerMissions.Clear();

            for (int i = 0; i < playerDatas.Count; i++)
            {
                var pd = playerDatas[i];
                string uid = pd.googleUID;

                // 사망자는 건너뜀
                var igd = GameDataManager.Instance.GetInGameData(uid);
                if (igd == null || !igd.isAlive) continue;

                var assigned = GetUniqueMissionsForPlayer(uid, missionsPerPlayer);
                if (assigned == null || assigned.Count == 0)
                {
                    Debug.LogWarning($"[Mission] No missions assigned for {uid}");
                    continue;
                }

                var missionData = new PlayerMissionData
                {
                    playerUID = uid,
                    missionList = assigned
                };
                allPlayerMissions[uid] = missionData;

                if (PhotonNetwork.CurrentRoom != null &&
                    PhotonNetwork.CurrentRoom.Players != null &&
                    PhotonNetwork.CurrentRoom.Players.TryGetValue(pd.currentActorId, out Photon.Realtime.Player target))
                {
                    photonView.RPC(nameof(RPC_AssignMissionList), target, JsonUtility.ToJson(missionData));
                }
                else
                {
                    Debug.LogWarning($"[Mission] Target actor {pd.currentActorId} not found.");
                }
            }

            GameDataManager.Instance.SetMissionBackup(allPlayerMissions);
        }

        /// <summary>
        /// 유니크 미션 k개 추출(Fisher–Yates)
        /// </summary>
        private List<MissionInstance> GetUniqueMissionsForPlayer(string playerUID, int count)
        {
            int total = (availableMissions != null) ? availableMissions.Count : 0;
            if (total <= 0)
            {
                Debug.LogWarning("[Mission] availableMissions is empty.");
                return null;
            }

            int k = Mathf.Clamp(count, 1, total);

            // 인덱스 배열 생성 후 k만큼 셔플 스왑
            var indices = ArrayPool<int>.Rent(total);
            try
            {
                for (int i = 0; i < total; i++) indices[i] = i;

                for (int i = 0; i < k; i++)
                {
                    int r = UnityEngine.Random.Range(i, total);
                    (indices[i], indices[r]) = (indices[r], indices[i]);
                }

                var list = new List<MissionInstance>(k);
                for (int i = 0; i < k; i++)
                {
                    var tmpl = availableMissions[indices[i]];
                    list.Add(new MissionInstance
                    {
                        missionId = tmpl.miniGameName,
                        missionType = tmpl.miniGameType,
                        isCompleted = false
                    });
                }
                return list;
            }
            finally
            {
                ArrayPool<int>.Return(indices);
            }
        }

        // --- CLIENT: 내 미션 수신/적용 ---

        [PunRPC]
        private void RPC_AssignMissionList(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<PlayerMissionData>(json);
            myMissions.Clear();
            if (data?.missionList != null) myMissions.AddRange(data.missionList);

            ApplyMyMissionList();
        }

        private void ApplyMyMissionList()
        {
            ClearAllMissionUI();

            var needed = HashSetPool<MiniGameType>.Get();
            try
            {
                for (int i = 0; i < myMissions.Count; i++)
                    needed.Add(myMissions[i].missionType);

                foreach (var kv in missionObjectMap)
                {
                    bool on = needed.Contains(kv.Key);
                    if (kv.Value && kv.Value.gameObject)
                        kv.Value.gameObject.SetActive(on);

                    // ★ 수정: 내 미션에 해당하는 것만 UI 생성
                    if (!on) continue;

                    if (!missionTaskInfoContent || !missionTaskContainerRect)
                    {
                        Debug.LogWarning("[Mission] UI prefab/container missing.");
                        continue;
                    }

                    if (!TryGetMissionInfo(kv.Key, out var missionInfoText))
                    {
                        Debug.LogWarning($"[Mission] No mission info for {kv.Key}");
                        continue;
                    }

                    var go = Instantiate(missionTaskInfoContent, missionTaskContainerRect);
                    var item = go.GetComponent<MissionTaskItem>() ?? go.AddComponent<MissionTaskItem>();
                    item.Init(kv.Key, missionInfoText);

                    missionUIMap[kv.Key] = item;
                }
            }
            finally
            {
                HashSetPool<MiniGameType>.Release(needed);
            }
        }

        public void ClearAllMissionUI()
        {
            // 생성된 UI 오브젝트 제거
            foreach (var kv in missionUIMap)
            {
                if (kv.Value && kv.Value.gameObject)
                    Destroy(kv.Value.gameObject);
            }

            missionUIMap.Clear();
        }

        // --- CLIENT → HOST: 미션 완료 요청 ---

        public void RequestMissionComplete(int playerId, int miniGameTypeInt)
        {
            photonView.RPC(nameof(RPC_UpdateMissionStatus), RpcTarget.MasterClient, playerId, miniGameTypeInt);
        }

        // --- HOST: 미션 완료 처리 ---

        [PunRPC]
        public void RPC_UpdateMissionStatus(int actorId, int miniGameTypeInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 발신자 검증(스푸핑 방지)
            if (info.Sender == null || info.Sender.ActorNumber != actorId)
            {
                Debug.LogWarning($"[Mission] Spoofed mission update? sender={info.Sender?.ActorNumber}, arg={actorId}");
                return;
            }

            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorId, out PublicPlayerData pdata))
            {
                UpdateMissionStatus(pdata.googleUID, miniGameTypeInt);
            }
            else
            {
                Debug.LogWarning($"[Mission] PublicPlayerData not found for actor {actorId}");
            }
        }

        public void UpdateMissionStatus(string playerId, int miniGameTypeInt)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (string.IsNullOrEmpty(playerId)) return;

            if (!allPlayerMissions.TryGetValue(playerId, out var pmd) || pmd?.missionList == null)
                return;

            var type = (MiniGameType)miniGameTypeInt;

            // 리스트에서 아직 완료되지 않은 해당 타입 1개만 완료 처리(구조체/클래스 모두 대응)
            int foundIdx = -1;
            for (int i = 0; i < pmd.missionList.Count; i++)
            {
                var m = pmd.missionList[i];
                if (m.missionType == type && !m.isCompleted)
                {
                    foundIdx = i;
                    break;
                }
            }
            if (foundIdx < 0)
            {
                Debug.LogWarning($"[Mission] {playerId} has no pending mission of {type}.");
                return;
            }

            var mission = pmd.missionList[foundIdx];
            mission.isCompleted = true;
            pmd.missionList[foundIdx] = mission; // struct 대비

            // 대상 클라 찾아서 개인 미션 최신 상태 전달
            if (GameDataManager.Instance.TryGetPublicPlayerData(playerId, out var pdata))
            {
                if (PhotonNetwork.CurrentRoom.Players.TryGetValue(pdata.currentActorId, out Photon.Realtime.Player target))
                {
                    string json = JsonUtility.ToJson(pmd);
                    photonView.RPC(nameof(Rpc_UpdateMissionStatus), target, json);
                    GiveMissionReward(playerId, target);
                }
            }

            // 전체 진행도 브로드캐스트
            float percent = GetMissionCompletePercent();
            photonView.RPC(nameof(Rpc_UpdateMissionStatusBar), RpcTarget.AllViaServer, percent);

            // 백업 갱신
            allPlayerMissions[playerId] = pmd;
            GameDataManager.Instance.SetMissionBackup(allPlayerMissions);
        }

        [PunRPC]
        public void Rpc_UpdateMissionStatus(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            var mission = JsonUtility.FromJson<PlayerMissionData>(json);
            if (mission?.missionList == null) return;

            // 내 태그 토글(미션별)
            for (int i = 0; i < mission.missionList.Count; i++)
            {
                var mi = mission.missionList[i];
                if (!missionObjectMap.TryGetValue(mi.missionType, out var tag) || !tag)
                    continue;

                tag.gameObject.SetActive(!mi.isCompleted); 

                missionUIMap[mi.missionType].SetCompleted(mi.isCompleted);
            }
        }

        private float GetMissionCompletePercent()
        {
            int total = 0;
            int done = 0;

            foreach (var kv in allPlayerMissions)
            {
                var list = kv.Value?.missionList;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    total++;
                    if (list[i].isCompleted) done++;
                }
            }
            if (total == 0) return 0f;
            return (float)done / total;
        }

        [PunRPC]
        public void Rpc_UpdateMissionStatusBar(float percent)
        {
            UIManager.Instance?.UpdateMissionStatusBarUI(percent);
        }

        private void GiveMissionReward(string playerUID, Photon.Realtime.Player target)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (string.IsNullOrEmpty(playerUID) || target == null) return;

            bool success = GameDataManager.Instance.TryAddMissionReward(playerUID);
            int coinAmount = GameDataManager.Instance.GetInGameData(playerUID)?.stickerCount ?? 0;

            if (success)
            {
                photonView.RPC(nameof(Rpc_ReceiveReward), target, coinAmount);
            }
        }

        [PunRPC]
        private void Rpc_ReceiveReward(int coinAmout)
        {
            UIManager.Instance.UpdateCoinInfo(coinAmout);
        }

        // --- 간단한 Array/HashSet 풀 (GC 감소용) ---

        private static class ArrayPool<T>
        {
            [ThreadStatic] private static T[] _buffer;
            public static T[] Rent(int size)
            {
                if (_buffer == null || _buffer.Length < size) _buffer = new T[size];
                return _buffer;
            }
            public static void Return(T[] array) { /* no-op (TLS 재사용) */ }
        }

        private static class HashSetPool<T>
        {
            [ThreadStatic] private static HashSet<T> _set;
            public static HashSet<T> Get() => _set ??= new HashSet<T>();
            public static void Release(HashSet<T> set) => set.Clear();
        }

        private bool TryGetMissionInfo(MiniGameType type, out string info)
        {
            info = null;
            if (availableMissions == null) return false;
            for (int i = 0; i < availableMissions.Count; i++)
            {
                if (availableMissions[i].miniGameType == type)
                {
                    info = availableMissions[i].missionInfo;
                    return true;
                }
            }
            return false;
        }
    }
}

