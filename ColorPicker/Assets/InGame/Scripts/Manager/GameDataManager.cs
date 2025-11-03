using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace ColorPicker.InGame
{
    public sealed class GameDataManager : SingletonNetworkBehaviour<GameDataManager>, IInRoomCallbacks
    {
        // ====== Core State ======
        private readonly Dictionary<string, PublicPlayerData> _publicByUid = new();     // UID -> Public
        private readonly Dictionary<string, PrivatePlayerData> _privateByUid = new();    // UID -> Private(호스트는 전원, 클라 로컬은 내 것만)
        private readonly Dictionary<string, InGameData> _inGameByUid = new();            // UID -> InGame

        private readonly Dictionary<string, int> _viewIdByUid = new();                  // UID -> ViewID
        private readonly Dictionary<int, string> _uidByViewId = new();                  // ViewID -> UID
        private readonly Dictionary<int, string> _uidByActor = new();                   // Actor -> UID (빠른 조회용)
        private readonly Dictionary<string, int> _actorByUid = new();                   // UID -> Actor

        private Dictionary<string, PlayerMissionData> _missionBackup;                   // 선택 사용

        private GameRuleSettings _rules = new GameRuleSettings();
        private GameStateType _gameState = GameStateType.None;

        // ====== Utility ======
        private bool IsHost => PhotonNetwork.IsMasterClient;

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                DumpMaps();
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            // 기본 룰 세팅
            _rules = new GameRuleSettings
            {
                mafiaAmount = 1,
                detectiveAmount = 0,
                paintCost = 1
            };

            // Photon 콜백 등록 (중복 등록 방지)
            if (PhotonNetwork.NetworkingClient != null)
                PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            if (PhotonNetwork.NetworkingClient != null)
                PhotonNetwork.RemoveCallbackTarget(this);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/DumpMaps")]
        private void DumpMaps()
        {
            Debug.Log("[GameDataManager] ==== UID↔Actor ====");
            foreach (var kv in _actorByUid) Debug.Log($"UID={kv.Key} -> Actor={kv.Value}");
            Debug.Log("[GameDataManager] ==== UID↔ViewID ====");
            foreach (var kv in _viewIdByUid) Debug.Log($"UID={kv.Key} -> ViewID={kv.Value}");
            Debug.Log("[GameDataManager] ==== ViewID↔UID ====");
            foreach (var kv in _uidByViewId) Debug.Log($"ViewID={kv.Key} -> UID={kv.Value}");
        }
#endif

        // ======================================================================
        #region [호스트 전용] Player Data 생성/갱신/재접속

        /// <summary>새로 접속한 플레이어 데이터 생성 (Host Only)</summary>
        public void CreateNewPlayerData(string uid, int actorNumber, int viewID)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] CreateNewPlayerData on non-host."); return; }
            if (string.IsNullOrEmpty(uid)) { Debug.LogWarning("[GameDataManager] CreateNewPlayerData invalid uid."); return; }

            // Public upsert
            if (!_publicByUid.TryGetValue(uid, out var pub))
            {
                pub = new PublicPlayerData
                {
                    googleUID = uid,
                    currentActorId = actorNumber,
                    nickname = $"Player_{actorNumber}",
                    customizationData = new PlayerCustomizationData()
                };
                _publicByUid[uid] = pub;
            }
            else
            {
                pub.currentActorId = actorNumber;
            }

            // Private upsert(호스트는 전원)
            if (!_privateByUid.TryGetValue(uid, out var prv))
            {
                prv = new PrivatePlayerData
                {
                    googleUID = uid,
                    currentActorId = actorNumber,
                    classType = (int)PlayerClassType.citizen,
                    identityColorId = (int)ColorType.White,
                };
                _privateByUid[uid] = prv;
            }
            else
            {
                prv.currentActorId = actorNumber;
            }

            // 맵 정합성 업데이트
            _actorByUid[uid] = actorNumber;
            _uidByActor[actorNumber] = uid;

            _viewIdByUid[uid] = viewID;
            _uidByViewId[viewID] = uid;

            // InGame upsert
            if (!_inGameByUid.TryGetValue(uid, out var ig) || ig == null)
                _inGameByUid[uid] = new InGameData { isAlive = true, hasVoted = false };
        }

        /// <summary>재접속 플레이어의 ActorNumber 갱신 (Host Only)</summary>
        public void HandleRejoinPlayer(string uid, int newActor)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] HandleRejoinPlayer on non-host."); return; }
            if (string.IsNullOrEmpty(uid)) { Debug.LogWarning("[GameDataManager] Rejoin invalid uid."); return; }

            if (_publicByUid.TryGetValue(uid, out var pub)) pub.currentActorId = newActor;
            if (_privateByUid.TryGetValue(uid, out var prv)) prv.currentActorId = newActor;

            _actorByUid[uid] = newActor;
            _uidByActor[newActor] = uid;

            // 최신 스냅샷은 호출부에서 SendFullSnapshotTo(target) 권장
        }

        /// <summary>퍼블릭 데이터 갱신 (Host Only)</summary>
        public void UpdatePublicPlayerData(PublicPlayerData newData)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] UpdatePublicPlayerData on non-host."); return; }
            if (newData == null || string.IsNullOrEmpty(newData.googleUID)) { Debug.LogWarning("[GameDataManager] UpdatePublicPlayerData invalid input."); return; }

            _publicByUid[newData.googleUID] = newData;

            // Actor 맵 갱신
            _actorByUid[newData.googleUID] = newData.currentActorId;
            _uidByActor[newData.currentActorId] = newData.googleUID;
        }

        /// <summary>프라이빗 데이터 갱신 (Host Only)</summary>
        public void UpdatePrivatePlayerData(PrivatePlayerData newData)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] UpdatePrivatePlayerData on non-host."); return; }
            if (newData == null || string.IsNullOrEmpty(newData.googleUID)) { Debug.LogWarning("[GameDataManager] UpdatePrivatePlayerData invalid input."); return; }

            _privateByUid[newData.googleUID] = newData;

            // Actor 맵 갱신
            _actorByUid[newData.googleUID] = newData.currentActorId;
            _uidByActor[newData.currentActorId] = newData.googleUID;
        }

        /// <summary>InGameData 초기화(Upsert)</summary>
        public void InitializedPlayerInGameData()
        {
            foreach (var uid in _publicByUid.Keys)
            {
                if (!_inGameByUid.TryGetValue(uid, out var ig) || ig == null)
                    _inGameByUid[uid] = new InGameData();

                ig = _inGameByUid[uid];
                ig.hasVoted = false;
                ig.isAlive = true;
            }
        }

        #endregion

        // ======================================================================
        #region InGame 델타(투표/생존) 전파
        // 요청: 로컬에서 바로 처리(호스트) / 비호스트는 호스트에 RPC
        public void RequestSetPlayerVoted(string uid, bool voted)
        {
            if (IsHost) SetPlayerVoted(uid, voted);
            else photonView.RPC(nameof(RPC_RequestSetPlayerVoted), RpcTarget.MasterClient, uid, voted);
        }

        public void RequestSetPlayerAliveState(string uid, bool isAlive)
        {
            if (IsHost) SetPlayerAliveState(uid, isAlive);
            else photonView.RPC(nameof(RPC_RequestSetPlayerAliveState), RpcTarget.MasterClient, uid, isAlive);
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

        /// <summary>Host: 내부 상태 변경 + 델타 브로드캐스트</summary>
        public void SetPlayerVoted(string uid, bool voted)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetPlayerVoted on non-host."); return; }
            if (string.IsNullOrEmpty(uid)) return;

            var ig = GetOrCreateInGame(uid);
            ig.hasVoted = voted;

            photonView.RPC(nameof(RPC_ApplyDeltaVoted), RpcTarget.Others, uid, voted);
        }

        /// <summary>Host: 내부 상태 변경 + 델타 브로드캐스트</summary>
        public void SetPlayerAliveState(string uid, bool isAlive)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetPlayerAliveState on non-host."); return; }
            if (string.IsNullOrEmpty(uid)) return;

            var ig = GetOrCreateInGame(uid);
            ig.isAlive = isAlive;

            photonView.RPC(nameof(RPC_ApplyDeltaAlive), RpcTarget.Others, uid, isAlive);
        }

        [PunRPC]
        private void RPC_ApplyDeltaVoted(string uid, bool voted)
        {
            if (string.IsNullOrEmpty(uid)) return;

            var ig = GetOrCreateInGame(uid);
            ig.hasVoted = voted;

            // ※ InGameData가 struct(값형)이라면:
            // _inGameByUid[uid] = ig;  // 재대입 필요
        }

        [PunRPC]
        private void RPC_ApplyDeltaAlive(string uid, bool isAlive)
        {
            if (string.IsNullOrEmpty(uid)) return;

            var ig = GetOrCreateInGame(uid);
            ig.isAlive = isAlive;

            // ※ InGameData가 struct(값형)이라면:
            // _inGameByUid[uid] = ig;  // 재대입 필요
        }

        private InGameData GetOrCreateInGame(string uid)
        {
            if (!_inGameByUid.TryGetValue(uid, out var ig) || ig == null)
            {
                ig = new InGameData();
                _inGameByUid[uid] = ig;
            }
            return ig; // ref 반환 금지(사전 인덱서는 ref 불가)
        }

        #endregion

        // ======================================================================
        #region 스냅샷 동기화 (브로드캐스트 / 타겟 전송 / 적용)

        public void SyncAllPlayerDataToClients()
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SyncAllPlayerDataToClients on non-host."); return; }

            try
            {
                var payload = BuildBackupPayload_NoLinq();
                var json = JsonUtility.ToJson(payload);
                photonView.RPC(nameof(RPC_ReceiveBackup), RpcTarget.Others, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] SyncAllPlayerDataToClients failed: {ex}");
            }
        }

        public void SendFullSnapshotTo(Photon.Realtime.Player target)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SendFullSnapshotTo on non-host."); return; }
            if (target == null) { Debug.LogWarning("[GameDataManager] SendFullSnapshotTo target null."); return; }

            try
            {
                var payload = BuildBackupPayload_NoLinq();
                var json = JsonUtility.ToJson(payload);
                photonView.RPC(nameof(RPC_ReceiveBackup), target, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] SendFullSnapshotTo failed: {ex}");
            }
        }

        private BackupPayload BuildBackupPayload_NoLinq()
        {
            var payload = new BackupPayload
            {
                publicPlayerDataList = new List<PublicPlayerData>(_publicByUid.Count),
                inGameDataList = new List<InGameDataEntry>(_inGameByUid.Count),
                encryptedPrivatePlayerDataList = new List<string>(_privateByUid.Count),
                viewIDMappings = new List<ViewIDEntry>(_viewIdByUid.Count),
                gameState = _gameState
            };

            // Public
            foreach (var kv in _publicByUid)
                payload.publicPlayerDataList.Add(kv.Value);

            // InGame
            foreach (var kv in _inGameByUid)
            {
                payload.inGameDataList.Add(new InGameDataEntry
                {
                    uid = kv.Key,
                    data = kv.Value ?? new InGameData()
                });
            }

            // Private(암호화)
            var key = AESKeyManager.Instance.GetAESKeyBytes();
            foreach (var kv in _privateByUid)
            {
                try
                {
                    string json = JsonUtility.ToJson(kv.Value);
                    string enc = AESCrypto.EncryptString(json, key);
                    payload.encryptedPrivatePlayerDataList.Add(enc);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[GameDataManager] Private encrypt fail (uid={kv.Key}): {e.Message}");
                }
            }

            // ViewID map
            foreach (var kv in _viewIdByUid)
            {
                payload.viewIDMappings.Add(new ViewIDEntry
                {
                    googleUID = kv.Key,
                    viewID = kv.Value
                });
            }

            return payload;
        }

        [PunRPC]
        private void RPC_ReceiveBackup(string json)
        {
            if (string.IsNullOrEmpty(json)) { Debug.LogWarning("[GameDataManager] RPC_ReceiveBackup empty json."); return; }

            try
            {
                var payload = JsonUtility.FromJson<BackupPayload>(json);
                if (payload == null) { Debug.LogWarning("[GameDataManager] RPC_ReceiveBackup payload null."); return; }

                LocalBackupManager.Instance.StoreBackupFromHost(payload);
                ApplyPublicPlayerData(payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] RPC_ReceiveBackup parse/apply failed: {ex}");
            }
        }

        /// <summary>수신된 스냅샷을 안전하게 적용(클라/호스트 공용)</summary>
        public void ApplyPublicPlayerData(BackupPayload payload)
        {
            if (payload == null) { Debug.LogWarning("[GameDataManager] ApplyPublicPlayerData payload null."); return; }

            // 전체 초기화 후 재적용(정합성 보장)
            _publicByUid.Clear();
            _inGameByUid.Clear();
            _viewIdByUid.Clear();
            _uidByViewId.Clear();
            _uidByActor.Clear();
            _actorByUid.Clear();

            // Public
            if (payload.publicPlayerDataList != null)
            {
                for (int i = 0; i < payload.publicPlayerDataList.Count; i++)
                {
                    var p = payload.publicPlayerDataList[i];
                    if (p == null || string.IsNullOrEmpty(p.googleUID)) continue;
                    _publicByUid[p.googleUID] = p;

                    _uidByActor[p.currentActorId] = p.googleUID;
                    _actorByUid[p.googleUID] = p.currentActorId;
                }
            }

            // InGame
            if (payload.inGameDataList != null)
            {
                for (int i = 0; i < payload.inGameDataList.Count; i++)
                {
                    var e = payload.inGameDataList[i];
                    if (e == null || string.IsNullOrEmpty(e.uid)) continue;
                    _inGameByUid[e.uid] = e.data ?? new InGameData();
                }
            }

            // ViewID
            if (payload.viewIDMappings != null)
            {
                for (int i = 0; i < payload.viewIDMappings.Count; i++)
                {
                    var m = payload.viewIDMappings[i];
                    if (m == null || string.IsNullOrEmpty(m.googleUID)) continue;

                    _viewIdByUid[m.googleUID] = m.viewID;
                    _uidByViewId[m.viewID] = m.googleUID;
                }
            }

            _gameState = payload.gameState;

            // 내 프라이빗만 복호화
            TryApplyMyPrivateDataFrom(payload);

            Debug.Log($"[GameDataManager] Snapshot applied. public={_publicByUid.Count}, inGame={_inGameByUid.Count}, viewMap={_viewIdByUid.Count}");
        }

        private void TryApplyMyPrivateDataFrom(BackupPayload payload)
        {
            try
            {
                // 내 UID
                string myUid = null;
                int myActor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
                if (myActor > 0) _uidByActor.TryGetValue(myActor, out myUid);
                if (string.IsNullOrEmpty(myUid)) return;

                var list = payload.encryptedPrivatePlayerDataList;
                if (list == null || list.Count == 0) return;

                var key = AESKeyManager.Instance.GetAESKeyBytes();
                for (int i = 0; i < list.Count; i++)
                {
                    var enc = list[i];
                    if (string.IsNullOrEmpty(enc)) continue;

                    try
                    {
                        string json = AESCrypto.DecryptString(enc, key);
                        var priv = JsonUtility.FromJson<PrivatePlayerData>(json);
                        if (priv != null && priv.googleUID == myUid)
                        {
                            _privateByUid[myUid] = priv; // 로컬 캐시
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[GameDataManager] Decrypt fail idx={i}: {e.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameDataManager] TryApplyMyPrivateDataFrom failed: {ex.Message}");
            }
        }

        #endregion

        // ======================================================================
        #region 조회 API (UID/Actor/ViewID/Rules/State)

        public bool TryGetPublicPlayerData(string uid, out PublicPlayerData data) => _publicByUid.TryGetValue(uid, out data);

        public bool TryGetPublicPlayerDataByActorId(int actorNum, out PublicPlayerData data)
        {
            data = null;
            if (_uidByActor.TryGetValue(actorNum, out var uid))
                return _publicByUid.TryGetValue(uid, out data);
            return false;
        }

        public bool TryGetPrivatePlayerData(string uid, out PrivatePlayerData data) => _privateByUid.TryGetValue(uid, out data);

        public bool TryGetPrivatePlayerDataByActorId(int actorNum, out PrivatePlayerData data)
        {
            data = null;
            if (_uidByActor.TryGetValue(actorNum, out var uid))
                return _privateByUid.TryGetValue(uid, out data);
            return false;
        }

        public bool TryGetMyPrivateData(out PrivatePlayerData data)
        {
            data = null;
            int myActor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
            if (myActor <= 0) return false;
            if (!_uidByActor.TryGetValue(myActor, out var uid) || string.IsNullOrEmpty(uid)) return false;
            return _privateByUid.TryGetValue(uid, out data);
        }

        public List<PublicPlayerData> GetAllPublicPlayerData()
        {
            var list = new List<PublicPlayerData>(_publicByUid.Count);
            foreach (var kv in _publicByUid) list.Add(kv.Value);
            return list;
        }

        public List<PrivatePlayerData> GetAllPrivatePlayerData()
        {
            var list = new List<PrivatePlayerData>(_privateByUid.Count);
            foreach (var kv in _privateByUid) list.Add(kv.Value);
            return list;
        }

        public InGameData GetInGameData(string uid)
        {
            _inGameByUid.TryGetValue(uid, out var data);
            return data;
        }

        public bool TryGetInGameDataByActorId(int actorNum, out InGameData data)
        {
            data = null;
            if (_uidByActor.TryGetValue(actorNum, out var uid) && !string.IsNullOrEmpty(uid))
            {
                _inGameByUid.TryGetValue(uid, out data);
                return data != null;
            }
            return false;
        }

        public bool TryGetViewIDByUID(string uid, out int viewID) => _viewIdByUid.TryGetValue(uid, out viewID);
        public bool TryGetUIDByViewID(int viewID, out string uid) => _uidByViewId.TryGetValue(viewID, out uid);

        public bool IsGameStarted() => _gameState != GameStateType.None;

        public void SetGameState(GameStateType state)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetGameState on non-host."); return; }
            _gameState = state;
        }

        public void SetGameRules(GameRuleSettings settings)
        {
            if (!IsHost) { Debug.LogWarning("[GameDataManager] SetGameRules on non-host."); return; }
            if (settings == null) { Debug.LogWarning("[GameDataManager] SetGameRules null."); return; }
            _rules = settings;
        }

        public GameRuleSettings GetGameRulePayload()
        {
            return new GameRuleSettings
            {
                mafiaAmount = _rules.mafiaAmount,
                detectiveAmount = _rules.detectiveAmount,
                paintCost = _rules.paintCost,
            };
        }

        public GameRuleSettings GetGameRules() => _rules;

        #endregion

        // ======================================================================
        #region 미션/보상 & 유틸

        public void SetMissionBackup(Dictionary<string, PlayerMissionData> backup)
        {
            // 필요 시 깊은 복사 고려
            _missionBackup = backup;
        }

        public bool TryAddMissionReward(string uid)
        {
            if (!_inGameByUid.TryGetValue(uid, out var ig) || ig == null)
            {
                Debug.LogWarning($"[GameDataManager] TryAddMissionReward: UID not found ({uid})");
                return false;
            }
            ig.coinCount++;
            return true;
        }

        /// <summary>로컬 백업이 있을 때 즉시 재적용(클라 전용)</summary>
        public void RequestResyncForLocal()
        {
            if (LocalBackupManager.Instance != null && LocalBackupManager.Instance.HasValidBackup())
                ApplyPublicPlayerData(LocalBackupManager.Instance.GetBackupPayload());
            else
                Debug.LogWarning("[GameDataManager] RequestResyncForLocal: no local backup.");
        }

        #endregion

        // ======================================================================
        #region Paint Map

        // viewID -> colorType
        private readonly Dictionary<int, int> _paintColorByView = new();

        public void RegistPaintObject(int viewID)
        {
            if (!_paintColorByView.ContainsKey(viewID))
                _paintColorByView[viewID] = -1;
        }

        public void UnregisterPaintObject(int viewID)
        {
            _paintColorByView.Remove(viewID);
        }

        public void SetPaintObjectColor(int viewID, int colorType)
        {
            _paintColorByView[viewID] = colorType;
        }

        public int GetPaintObjectColor(int viewID)
        {
            return _paintColorByView.TryGetValue(viewID, out var color) ? color : -1;
        }

        public Dictionary<int, int> GetAllPaintObjectDictionary()
        {
            return _paintColorByView; // 필요시 방어적 복사 고려
        }

        public bool CheckAllPainted()
        {
            foreach (var kv in _paintColorByView)
                if (kv.Value == -1) return false;
            return true;
        }

        #endregion

        // ======================================================================
        #region Photon Room Callbacks (정합성/정리)

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            // 호스트가 Late-Join에게 스냅샷 1:1 전송 권장
            if (IsHost && newPlayer != null)
                SendFullSnapshotTo(newPlayer);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (otherPlayer == null) return;

            // 맵 정리(Actor 기반)
            int actor = otherPlayer.ActorNumber;
            if (_uidByActor.TryGetValue(actor, out var uid) && !string.IsNullOrEmpty(uid))
            {
                // Public/InGame은 게임 로직에 따라 유지 가능(중도 이탈 후 복귀 대비)
                // 여기서는 맵만 부분 정리
                _uidByActor.Remove(actor);

                // ViewID는 PhotonView가 파괴되며 자연정리 될 수 있으나, 수동 정리 안전
                if (_viewIdByUid.TryGetValue(uid, out var viewId))
                {
                    _viewIdByUid.Remove(uid);
                    _uidByViewId.Remove(viewId);
                }
            }
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            // Host migration 정책에 따라: 새 호스트가 전체 스냅샷 재배포 가능
            if (IsHost)
                SyncAllPlayerDataToClients();
        }

        #endregion
    }
}
