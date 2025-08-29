using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // Player 타입 사용
// using ExitGames.Client.Photon; // RaiseEvent로 확장 시

namespace ColorPicker.InGame
{
    /// <summary>
    /// Host-Authoritative 데이터 허브
    /// - 기존 퍼블릭 API 유지
    /// - Late-Join/재접속 안전, 델타 전파, 스냅샷 적용 내성 강화
    /// </summary>
    public class GameDataManager : SingletonNetworkBehaviour<GameDataManager>
    {
        // ====== Core State ======
        private readonly Dictionary<string, PublicPlayerData>  publicPlayerDataDict  = new Dictionary<string, PublicPlayerData>();  // UID -> Public
        private readonly Dictionary<string, PrivatePlayerData> privatePlayerDataDict = new Dictionary<string, PrivatePlayerData>(); // UID -> Private (각 클라 로컬은 "본인 것만" 보유)
        private readonly Dictionary<string, InGameData>        inGameDataDict        = new Dictionary<string, InGameData>();        // UID -> InGame
        private Dictionary<string, PlayerMissionData> missionBackup;

        private readonly Dictionary<string, int> viewIDByGoogleUID = new Dictionary<string, int>();
        private readonly Dictionary<int, string> googleUIDByViewID = new Dictionary<int, string>();

        private GameRuleSettings currentGameRuleSettings = new GameRuleSettings();
        private GameStateType currentGameState = GameStateType.None;

        // ====== Utility ======
        private bool IsHost => PhotonNetwork.IsMasterClient;

        protected override void Awake()
        {
            base.Awake();

            // 기본 룰
            currentGameRuleSettings = new GameRuleSettings()
            {
                mafiaAmount = 1,
                detectiveAmount = 0
            };
        }

        #region [호스트 전용] Player Data 생성 및 갱신

        /// <summary>새로 접속한 플레이어 데이터 생성 (Host Only)</summary>
        public void CreateNewPlayerData(string googleUID, int actorNumber, int viewID)
        {
            if (!IsHost)
            {
                Debug.LogWarning("[GameDataManager] CreateNewPlayerData called on non-host. Ignored.");
                return;
            }

            if (string.IsNullOrEmpty(googleUID))
            {
                Debug.LogWarning("[GameDataManager] CreateNewPlayerData failed: googleUID is null/empty.");
                return;
            }

            var publicData = new PublicPlayerData
            {
                googleUID = googleUID,
                currentActorId = actorNumber,
                nickname = $"Player_{actorNumber}",
                customizationData = new PlayerCustomizationData()
            };

            var privateData = new PrivatePlayerData
            {
                googleUID = googleUID,
                currentActorId = actorNumber,
                classType = (int)PlayerClassType.citizen,
                identityColorId = (int)ColorType.White,
            };

            publicPlayerDataDict[googleUID]  = publicData;  // upsert
            privatePlayerDataDict[googleUID] = privateData; // upsert (호스트만 전체 보유)

            viewIDByGoogleUID[googleUID] = viewID; // upsert
            googleUIDByViewID[viewID]    = googleUID; // upsert
        }

        /// <summary>재접속 플레이어 ActorNumber 갱신 (Host Only)</summary>
        public void HandleRejoinPlayer(string googleUID, int newActorId)
        {
            if (!IsHost)
            {
                Debug.LogWarning("[GameDataManager] HandleRejoinPlayer on non-host. Ignored.");
                return;
            }

            if (!publicPlayerDataDict.TryGetValue(googleUID, out var publicData) ||
                !privatePlayerDataDict.TryGetValue(googleUID, out var privateData))
            {
                Debug.LogWarning($"[GameDataManager] Rejoin failed: no data for UID {googleUID}");
                return;
            }

            Debug.Log($"[GameDataManager] Rejoin start: UID={googleUID} -> Actor={newActorId}");

            publicData.currentActorId  = newActorId;
            privateData.currentActorId = newActorId;

            UpdatePublicPlayerData(publicData);
            UpdatePrivatePlayerData(privateData);

            // 권장: 재접속자에게 최신 스냅샷 1:1 전송 (호출부에서 Player를 넘길 수 있으면 아래 오버로드 사용)
            // SendFullSnapshotTo(targetPlayer);
        }

        /// <summary>퍼블릭 데이터 갱신 (Host Only)</summary>
        public void UpdatePublicPlayerData(PublicPlayerData newData)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] UpdatePublicPlayerData on non-host. Ignored."); return; }
            if (newData == null || string.IsNullOrEmpty(newData.googleUID)) { Debug.LogWarning("[GameDataManager] UpdatePublicPlayerData invalid input."); return; }

            if (publicPlayerDataDict.TryGetValue(newData.googleUID, out var existing))
            {
                existing.currentActorId    = newData.currentActorId;
                existing.nickname          = newData.nickname;
                existing.customizationData = newData.customizationData;
            }
            else
            {
                publicPlayerDataDict[newData.googleUID] = newData;
            }
        }

        /// <summary>프라이빗 데이터 갱신 (Host Only)</summary>
        public void UpdatePrivatePlayerData(PrivatePlayerData newData)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] UpdatePrivatePlayerData on non-host. Ignored."); return; }
            if (newData == null || string.IsNullOrEmpty(newData.googleUID)) { Debug.LogWarning("[GameDataManager] UpdatePrivatePlayerData invalid input."); return; }

            if (privatePlayerDataDict.TryGetValue(newData.googleUID, out var existing))
            {
                existing.classType       = newData.classType;
                existing.identityColorId = newData.identityColorId;
            }
            else
            {
                privatePlayerDataDict[newData.googleUID] = newData;
            }
        }

        /// <summary>InGameData 초기화(Upsert) — 중복 Add 예외 방지</summary>
        public void InitializedPlayerInGameData()
        {
            foreach (var uid in publicPlayerDataDict.Keys)
            {
                if (!inGameDataDict.TryGetValue(uid, out var data) || data == null)
                {
                    data = new InGameData();
                    inGameDataDict[uid] = data;
                }

                // 기획에 맞게 기본값 리셋
                data.hasVoted = false;
                data.isAlive  = true;
                // stickerCount 등은 유지/리셋 여부를 기획에 맞춰 결정. 기본은 유지.
            }
        }

        #endregion

        #region InGame 데이터 설정 (투표/생존 등) + 델타 전파

        public void RequestSetPlayerVoted(string uid, bool voted)
        {
            if (IsHost)
            {
                SetPlayerVoted(uid, voted);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestSetPlayerVoted), RpcTarget.MasterClient, uid, voted);
            }
        }

        public void RequestSetPlayerAliveState(string uid, bool isAlive)
        {
            if (IsHost)
            {
                SetPlayerAliveState(uid, isAlive);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestSetPlayerAliveState), RpcTarget.MasterClient, uid, isAlive);
            }
        }

        [PunRPC]
        private void RPC_RequestSetPlayerVoted(string uid, bool voted)
        {
            if (!IsHost) return;
            SetPlayerVoted(uid, voted);
        }

        [PunRPC]
        private void RPC_RequestSetPlayerAliveState(string uid, bool isAlive)
        {
            if (!IsHost) return;
            SetPlayerAliveState(uid, isAlive);
        }

        /// <summary>Host 전용: 내부 상태 변경 + 델타 브로드캐스트</summary>
        public void SetPlayerVoted(string uid, bool voted)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetPlayerVoted on non-host. Ignored."); return; }
            if (string.IsNullOrEmpty(uid)) { Debug.LogWarning("[GameDataManager] SetPlayerVoted invalid uid."); return; }

            if (!inGameDataDict.TryGetValue(uid, out var data) || data == null)
            {
                data = new InGameData();
                inGameDataDict[uid] = data;
            }

            data.hasVoted = voted;

            // 델타 전파
            photonView.RPC(nameof(RPC_ApplyDeltaVoted), RpcTarget.Others, uid, voted);
        }

        /// <summary>Host 전용: 내부 상태 변경 + 델타 브로드캐스트</summary>
        public void SetPlayerAliveState(string uid, bool isAlive)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetPlayerAliveState on non-host. Ignored."); return; }
            if (string.IsNullOrEmpty(uid)) { Debug.LogWarning("[GameDataManager] SetPlayerAliveState invalid uid."); return; }

            if (!inGameDataDict.TryGetValue(uid, out var data) || data == null)
            {
                data = new InGameData();
                inGameDataDict[uid] = data;
            }

            data.isAlive = isAlive;

            // 델타 전파
            photonView.RPC(nameof(RPC_ApplyDeltaAlive), RpcTarget.Others, uid, isAlive);
        }

        [PunRPC]
        private void RPC_ApplyDeltaVoted(string uid, bool voted)
        {
            if (string.IsNullOrEmpty(uid)) return;

            if (!inGameDataDict.TryGetValue(uid, out var data) || data == null)
            {
                data = new InGameData();
                inGameDataDict[uid] = data;
            }

            data.hasVoted = voted;
        }

        [PunRPC]
        private void RPC_ApplyDeltaAlive(string uid, bool isAlive)
        {
            if (string.IsNullOrEmpty(uid)) return;

            if (!inGameDataDict.TryGetValue(uid, out var data) || data == null)
            {
                data = new InGameData();
                inGameDataDict[uid] = data;
            }

            data.isAlive = isAlive;
        }

        #endregion

        #region 데이터 동기화 및 백업 처리 (스냅샷)

        /// <summary>
        /// 전체 스냅샷 브로드캐스트 (Others)
        /// - 기존 메서드 유지 (호환성)
        /// - Late-Join은 아래 SendFullSnapshotTo(Player) 사용 권장
        /// </summary>
        public void SyncAllPlayerDataToClients()
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SyncAllPlayerDataToClients on non-host. Ignored."); return; }

            try
            {
                var payload = BuildBackupPayload();
                string json = JsonUtility.ToJson(payload);
                photonView.RPC(nameof(RPC_ReceiveBackup), RpcTarget.Others, json);
                Debug.Log("[GameDataManager] Backup snapshot broadcasted to Others.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] SyncAllPlayerDataToClients failed: {ex}");
            }
        }

        /// <summary>
        /// Late-Join/재접속자에 대한 타겟 스냅샷 전송 (권장)
        /// - 호출부에서 OnPlayerEnteredRoom 등에서 Player를 받아 넘겨 사용
        /// </summary>
        public void SendFullSnapshotTo(Photon.Realtime.Player target)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SendFullSnapshotTo on non-host. Ignored."); return; }
            if (target == null) { Debug.LogWarning("[GameDataManager] SendFullSnapshotTo target null."); return; }

            try
            {
                var payload = BuildBackupPayload();
                string json = JsonUtility.ToJson(payload);
                photonView.RPC(nameof(RPC_ReceiveBackup), target, json);
                Debug.Log($"[GameDataManager] Backup snapshot sent to {target.ActorNumber}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] SendFullSnapshotTo failed: {ex}");
            }
        }

        private BackupPayload BuildBackupPayload()
        {
            // lists로 복제하여 직렬화 안전성 확보
            var publicDataList = publicPlayerDataDict.Values.ToList();
            var inGameDataList = inGameDataDict
                .Select(kv => new InGameDataEntry { uid = kv.Key, data = kv.Value })
                .ToList();

            var encryptedPrivateList = privatePlayerDataDict
                .Select(kv => AESCrypto.EncryptString(JsonUtility.ToJson(kv.Value), AESKeyManager.Instance.GetAESKeyBytes()))
                .ToList();

            var viewIDMappingList = viewIDByGoogleUID
                .Select(kv => new ViewIDEntry { googleUID = kv.Key, viewID = kv.Value })
                .ToList();

            var payload = new BackupPayload
            {
                publicPlayerDataList = publicDataList,
                inGameDataList = inGameDataList,
                encryptedPrivatePlayerDataList = encryptedPrivateList,
                gameState = currentGameState,
                viewIDMappings = viewIDMappingList
                // (선택 확장) gameRules, snapshotVersion 등 추가해도 기존과 호환됨
            };

            return payload;
        }

        /// <summary>[Client] 스냅샷 수신 → 로컬 저장 → 적용</summary>
        [PunRPC]
        private void RPC_ReceiveBackup(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[GameDataManager] RPC_ReceiveBackup: empty json.");
                return;
            }

            try
            {
                var payload = JsonUtility.FromJson<BackupPayload>(json);
                if (payload == null)
                {
                    Debug.LogWarning("[GameDataManager] RPC_ReceiveBackup: payload null after parse.");
                    return;
                }

                LocalBackupManager.Instance.StoreBackupFromHost(payload);
                ApplyPublicPlayerData(LocalBackupManager.Instance.GetBackupPayload());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] RPC_ReceiveBackup parse/apply failed: {ex}");
            }
        }

        /// <summary>수신된 스냅샷을 안전하게 적용</summary>
        public void ApplyPublicPlayerData(BackupPayload payload)
        {
            if (payload == null)
            {
                Debug.LogWarning("[GameDataManager] ApplyPublicPlayerData: payload null.");
                return;
            }

            // 딕셔너리 정합성 확보: Clear 후 재구성
            publicPlayerDataDict.Clear();
            inGameDataDict.Clear();
            viewIDByGoogleUID.Clear();
            googleUIDByViewID.Clear();

            if (payload.publicPlayerDataList != null)
            {
                foreach (var p in payload.publicPlayerDataList)
                {
                    if (p != null && !string.IsNullOrEmpty(p.googleUID))
                        publicPlayerDataDict[p.googleUID] = p;
                }
            }

            if (payload.inGameDataList != null)
            {
                foreach (var entry in payload.inGameDataList)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.uid))
                        inGameDataDict[entry.uid] = entry.data ?? new InGameData();
                }
            }

            // ViewID 매핑 반영
            if (payload.viewIDMappings != null)
            {
                foreach (var m in payload.viewIDMappings)
                {
                    if (m != null && !string.IsNullOrEmpty(m.googleUID))
                    {
                        viewIDByGoogleUID[m.googleUID] = m.viewID;
                        googleUIDByViewID[m.viewID]    = m.googleUID;
                    }
                }
            }

            currentGameState = payload.gameState;
            // (선택) if (payload.gameRules != null) currentGameRuleSettings = payload.gameRules;

            // ★ 내 프라이빗만 복호화→캐시 (클라이언트 로컬)
            TryApplyMyPrivateDataFrom(payload);

            // 방어용 로그
            Debug.Log($"[GameDataManager] Snapshot applied. public={publicPlayerDataDict.Count}, inGame={inGameDataDict.Count}, map={viewIDByGoogleUID.Count}");
        }

        /// <summary>
        /// payload의 encryptedPrivatePlayerDataList에서
        /// "내 것(내 ActorNumber→내 UID)"만 복호화하여 privatePlayerDataDict에 캐시
        /// </summary>
        private void TryApplyMyPrivateDataFrom(BackupPayload payload)
        {
            try
            {
                // 내 UID 찾기 (내 ActorNumber 기준)
                var me = publicPlayerDataDict.Values
                    .FirstOrDefault(p => p.currentActorId == PhotonNetwork.LocalPlayer.ActorNumber);
                if (me == null || string.IsNullOrEmpty(me.googleUID))
                    return;

                string myUid = me.googleUID;
                var encList = payload.encryptedPrivatePlayerDataList;
                if (encList == null || encList.Count == 0) return;

                foreach (var enc in encList)
                {
                    if (string.IsNullOrEmpty(enc)) continue;

                    string json = AESCrypto.DecryptString(enc, AESKeyManager.Instance.GetAESKeyBytes());
                    var priv = JsonUtility.FromJson<PrivatePlayerData>(json);
                    if (priv != null && priv.googleUID == myUid)
                    {
                        privatePlayerDataDict[myUid] = priv;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameDataManager] TryApplyMyPrivateDataFrom failed: {ex.Message}");
            }
        }

        #endregion

        #region 데이터 조회 인터페이스 (UID/Actor 기반)

        public bool TryGetPublicPlayerData(string uid, out PublicPlayerData data)
            => publicPlayerDataDict.TryGetValue(uid, out data);

        public bool TryGetPublicPlayerDataByActorId(int actorNum, out PublicPlayerData data)
        {
            data = publicPlayerDataDict.Values.FirstOrDefault(p => p.currentActorId == actorNum);
            return data != null;
        }

        public bool TryGetPrivatePlayerData(string uid, out PrivatePlayerData data)
            => privatePlayerDataDict.TryGetValue(uid, out data);

        public bool TryGetPrivatePlayerDataByActorId(int actorNum, out PrivatePlayerData data)
        {
            data = privatePlayerDataDict.Values.FirstOrDefault(p => p.currentActorId == actorNum);
            return data != null;
        }

        /// <summary>내 프라이빗 데이터 바로 얻기 (UI 등에서 편하게 사용)</summary>
        public bool TryGetMyPrivateData(out PrivatePlayerData data)
        {
            data = null;

            // 내 UID를 퍼블릭 테이블에서 찾고, 그 UID로 프라이빗 조회
            var me = publicPlayerDataDict.Values
                .FirstOrDefault(p => p.currentActorId == PhotonNetwork.LocalPlayer.ActorNumber);
            if (me == null || string.IsNullOrEmpty(me.googleUID))
                return false;

            return privatePlayerDataDict.TryGetValue(me.googleUID, out data);
        }

        public List<PublicPlayerData> GetAllPublicPlayerData()
            => publicPlayerDataDict.Values.ToList();

        public List<PrivatePlayerData> GetAllPrivatePlayerData()
            => privatePlayerDataDict.Values.ToList();

        public InGameData GetInGameData(string uid)
            => inGameDataDict.TryGetValue(uid, out var data) ? data : null;

        public bool TryGetViewIDByUID(string googleUID, out int viewID)
            => viewIDByGoogleUID.TryGetValue(googleUID, out viewID);

        public bool TryGetUIDByViewID(int viewID, out string googleUID)
            => googleUIDByViewID.TryGetValue(viewID, out googleUID);

        #endregion

        #region 게임 상태 및 룰 관리

        public bool IsGameStarted() => currentGameState != GameStateType.None;

        public void SetGameState(GameStateType state)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetGameState on non-host. Ignored."); return; }
            currentGameState = state;
        }

        public void SetGameRules(GameRuleSettings settings)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetGameRules on non-host. Ignored."); return; }
            if (settings == null) { Debug.LogWarning("[GameDataManager] SetGameRules null settings."); return; }
            currentGameRuleSettings = settings;
        }

        public GameRuleSettings GetGameRulePayload()
        {
            return new GameRuleSettings
            {
                mafiaAmount     = currentGameRuleSettings.mafiaAmount,
                detectiveAmount = currentGameRuleSettings.detectiveAmount,
            };
        }

        public GameRuleSettings GetGameRules() => currentGameRuleSettings;

        #endregion

        #region 미션/보상 & 유틸

        public void SetMissionBackup(Dictionary<string, PlayerMissionData> backup)
        {
            missionBackup = backup; // 필요 시 Deep Copy 고려
        }

        public bool TryAddMissionReward(string playerUID)
        {
            if (!inGameDataDict.ContainsKey(playerUID))
            {
                Debug.LogWarning($"[GameDataManager] TryAddMissionReward: UID not found ({playerUID})");
                return false;
            }

            inGameDataDict[playerUID].stickerCount++;
            return true;
        }

        /// <summary>
        /// 로컬에 백업이 있고 화면 요소가 먼저 올라와 레이스가 났을 때
        /// 즉시 재적용하고 싶을 때 호출 (클라 전용)
        /// </summary>
        public void RequestResyncForLocal()
        {
            if (LocalBackupManager.Instance != null && LocalBackupManager.Instance.HasValidBackup())
            {
                ApplyPublicPlayerData(LocalBackupManager.Instance.GetBackupPayload());
            }
            else
            {
                Debug.LogWarning("[GameDataManager] RequestResyncForLocal: no local backup.");
            }
        }

        #endregion
    }
}
