using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Minseok;

namespace ColorPicker.InGame
{
    public class GameDataManager : SingletonNetworkBehaviour<GameDataManager>
    {
        private Dictionary<string, PublicPlayerData> publicPlayerDataDict = new Dictionary<string, PublicPlayerData>(); // UID로 맵핑된 playerData
        private Dictionary<string, PrivatePlayerData> privatePlayerDataDict = new Dictionary<string, PrivatePlayerData>(); // UID로 맵핑된 playerData
        private Dictionary<string, InGameData> inGameDataDict = new Dictionary<string, InGameData>(); // UID로 맵핑된 InGameData

        private Dictionary<string, int> viewIDByGoogleUID = new Dictionary<string, int>();
        private Dictionary<int, string> googleUIDByViewID = new Dictionary<int, string>();

        private GameRuleSettings currentGameRuleSettings = new GameRuleSettings();  

        private GameStateType currentGameState = GameStateType.None; // 데이터 저장을 위한 enum class

        #region [호스트 전용] Player Data 생성 및 갱신 

        /// <summary>
        /// 새로 접속한 플레이어 데이터를 생성하는 함수
        /// </summary>
        /// <param name="googleUID">플레이어의 고유 식별자(Google UID)</param>
        /// <param name="actorNumber">Photon의 ActorNumber</param>
        public void CreateNewPlayerData(string googleUID, int actorNumber, int viewID)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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

            publicPlayerDataDict[googleUID] = publicData;
            privatePlayerDataDict[googleUID] = privateData;

            viewIDByGoogleUID[googleUID] = viewID;
            googleUIDByViewID[viewID] = googleUID;
        }

        /// <summary>
        /// 재접속한 플레이어 정보를 갱신하는 함수
        /// </summary>
        /// <param name="googleUID">플레이어 UID</param>
        /// <param name="newActorId">새 ActorNumber</param>
        public void HandleRejoinPlayer(string googleUID, int newActorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!publicPlayerDataDict.TryGetValue(googleUID, out var publicData) ||
                !privatePlayerDataDict.TryGetValue(googleUID, out var privateData))
            {
                Debug.LogWarning($"[GameDataManager] 재접속 처리 실패: UID {googleUID}에 대한 데이터가 존재하지 않음");
                return;
            }

            Debug.Log($"[GameDataManager] 재접속 처리 시작: {googleUID}");

            publicData.currentActorId = newActorId;
            privateData.currentActorId = newActorId;

            UpdatePublicPlayerData(publicData);
            UpdatePrivatePlayerData(privateData);
        }

        /// <summary>
        /// 퍼블릭 플레이어 데이터를 갱신하는 함수
        /// </summary>
        /// <param name="newData">PublicPlayerData</param>
        public void UpdatePublicPlayerData(PublicPlayerData newData)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (publicPlayerDataDict.TryGetValue(newData.googleUID, out var existing))
            {
                existing.currentActorId = newData.currentActorId;
                existing.nickname = newData.nickname;
                existing.customizationData = newData.customizationData;
            }
            else
            {
                publicPlayerDataDict[newData.googleUID] = newData;
            }
        }

        /// <summary>
        /// 프라이빗 플레이어 데이터를 갱신하는 함수
        /// </summary>
        /// <param name="newData">PrivatePlayerData</param>
        public void UpdatePrivatePlayerData(PrivatePlayerData newData)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (privatePlayerDataDict.TryGetValue(newData.googleUID, out var existing))
            {
                existing.classType = newData.classType;
                existing.identityColorId = newData.identityColorId;
            }
            else
            {
                privatePlayerDataDict[newData.googleUID] = newData;
            }
        }

        #endregion

        #region InGame 데이터 설정 (투표/생존 등)

        /// <summary>
        /// 투표 여부 설정을 호스트에게 요청하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="voted">투표 여부</param>
        public void RequestSetPlayerVoted(string uid, bool voted)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetPlayerVoted(uid, voted);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestSetPlayerVoted), RpcTarget.MasterClient, uid, voted);
            }
        }

        /// <summary>
        /// 생존 여부 설정을 호스트에게 요청하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="isAlive">생존 여부</param>
        public void RequestSetPlayerAliveState(string uid, bool isAlive)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetPlayerAliveState(uid, isAlive);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestSetPlayerAliveState), RpcTarget.MasterClient, uid, isAlive);
            }
        }

        /// <summary>
        /// [RPC][호스트 전용] 투표 여부 설정 요청을 처리하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="voted">투표 여부</param>
        [PunRPC]
        private void RPC_RequestSetPlayerVoted(string uid, bool voted)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            SetPlayerVoted(uid, voted);
        }

        /// <summary>
        /// 플레이어의 투표 여부를 설정하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="voted">투표 여부</param>
        public void SetPlayerVoted(string uid, bool voted)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!inGameDataDict.TryGetValue(uid, out var data))
            {
                data = new InGameData(); // 기본값으로 생성
                inGameDataDict[uid] = data;
            }

            data.hasVoted = voted;
        }

        /// <summary>
        /// [RPC][호스트 전용] 생존 여부 설정 요청을 처리하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="isAlive">생존 여부</param>
        [PunRPC]
        private void RPC_RequestSetPlayerAliveState(string uid, bool isAlive)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            SetPlayerAliveState(uid, isAlive);
        }

        /// <summary>
        /// 플레이어의 생존 여부를 설정하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="isAlive">생존 여부</param>
        public void SetPlayerAliveState(string uid, bool isAlive)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!inGameDataDict.TryGetValue(uid, out var data))
            {
                data = new InGameData(); // 기본값으로 생성
                inGameDataDict[uid] = data;
            }

            data.isAlive = isAlive;
        }

        #endregion

        #region 데이터 동기화 및 백업 처리

        /// <summary>
        /// 모든 플레이어 데이터를 백업 및 동기화하는 함수
        /// </summary>
        public void SyncAllPlayerDataToClients()
        {
            if (!PhotonNetwork.IsMasterClient) return;

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
            };

            string json = JsonUtility.ToJson(payload);
            photonView.RPC(nameof(RPC_ReceiveBackup), RpcTarget.Others, json);

            Debug.Log("[GameDataManager] 백업 데이터 전송 완료.");
        }

        /// <summary>
        /// [PunRPC] 백업 JSON을 수신해 적용하는 함수
        /// </summary>
        /// <param name="json">백업 JSON 문자열</param>
        [PunRPC]
        private void RPC_ReceiveBackup(string json)
        {
            var payload = JsonUtility.FromJson<BackupPayload>(json); // json 형식으로 받은 데이터를 payload로 변환
            LocalBackupManager.Instance.StoreBackupFromHost(payload); // 호스트로 부터 json 파일을 받아 백업 데이터로 저장

            ApplyPublicPlayerData(LocalBackupManager.Instance.GetBackupPayload());
        }

        /// <summary>
        /// 수신된 백업 데이터를 적용하는 함수
        /// </summary>
        /// <param name="payload">백업 페이로드</param>
        public void ApplyPublicPlayerData(BackupPayload payload)
        {
            publicPlayerDataDict = payload.publicPlayerDataList.ToDictionary(p => p.googleUID);
            inGameDataDict = payload.inGameDataList.ToDictionary(entry => entry.uid, entry => entry.data);

            currentGameState = payload.gameState;
        }

        #endregion

        #region 데이터 조회 인터페이스 (UID/Actor 기반)

        /// <summary>
        /// UID로 퍼블릭 데이터를 조회하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="data">조회 결과</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetPublicPlayerData(string uid, out PublicPlayerData data)
        {
            return publicPlayerDataDict.TryGetValue(uid, out data);
        }

        /// <summary>
        /// ActorNumber로 퍼블릭 데이터를 조회하는 함수
        /// </summary>
        /// <param name="actorNum">Photon ActorNumber</param>
        /// <param name="data">조회 결과</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetPublicPlayerDataByActorId(int actorNum, out PublicPlayerData data)
        {
            data = publicPlayerDataDict.Values.FirstOrDefault(p => p.currentActorId == actorNum);
            return data != null;
        }

        /// <summary>
        /// UID로 프라이베이트 데이터를 조회하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <param name="data">조회 결과</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetPrivatePlayerData(string uid, out PrivatePlayerData data)
        {
            return privatePlayerDataDict.TryGetValue(uid, out data);
        }

        /// <summary>
        /// ActorNumber로 프라이베이트 데이터를 조회하는 함수
        /// </summary>
        /// <param name="actorNum">Photon ActorNumber</param>
        /// <param name="data">조회 결과</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetPrivatePlayerDataByActorId(int actorNum, out PrivatePlayerData data)
        {
            data = privatePlayerDataDict.Values.FirstOrDefault(p => p.currentActorId == actorNum);
            return data != null;
        }

        /// <summary>
        /// 모든 퍼블릭 데이터를 리스트로 반환하는 함수
        /// </summary>
        /// <returns>PublicPlayerData 리스트</returns>
        public List<PublicPlayerData> GetAllPublicPlayerData()
        {
            return publicPlayerDataDict.Values.ToList();
        }

        /// <summary>
        /// 모든 프라이베이트 데이터를 리스트로 반환하는 함수
        /// </summary>
        /// <returns>PublicPlayerData 리스트</returns>
        public List<PrivatePlayerData> GetAllPrivatePlayerData()
        {
            return privatePlayerDataDict.Values.ToList();
        }

        /// <summary>
        /// UID로 인게임 데이터를 조회하는 함수
        /// </summary>
        /// <param name="uid">플레이어 UID</param>
        /// <returns>InGameData 또는 null</returns>
        public InGameData GetInGameData(string uid)
        {
            return inGameDataDict.TryGetValue(uid, out var data) ? data : null;
        }

        /// <summary>
        /// [호스트 전용] Google UID로 ViewID를 조회하는 함수
        /// </summary>
        /// <param name="googleUID">플레이어 Google UID</param>
        /// <param name="viewID">조회된 ViewID</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetViewIDByUID(string googleUID, out int viewID)
        {
            return viewIDByGoogleUID.TryGetValue(googleUID, out viewID);
        }

        /// <summary>
        /// [호스트 전용] ViewID로 Google UID를 조회하는 함수
        /// </summary>
        /// <param name="googleUID">플레이어 Google UID</param>
        /// <param name="viewID">조회된 ViewID</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetUIDByViewID(int viewID, out string googleUID)
        {
            return googleUIDByViewID.TryGetValue(viewID, out googleUID);
        }
        #endregion

        #region 게임 상태 및 룰 관리

        /// <summary>
        /// 게임이 시작되었는지 확인하는 함수
        /// </summary>
        /// <returns>시작 여부</returns>
        public bool IsGameStarted()
        {
            return currentGameState != GameStateType.None;
        }

        /// <summary>
        /// 게임 상태를 설정하는 함수
        /// </summary>
        /// <param name="state">GameStateType</param>
        public void SetGameState(GameStateType state)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            currentGameState = state;
        }

        /// <summary>
        /// 게임 룰을 설정하는 함수
        /// </summary>
        /// <param name="settings">GameRuleSettings</param>
        public void SetGameRules(GameRuleSettings settings)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            currentGameRuleSettings = settings;
        }

        /// <summary>
        /// 룰의 복사본을 반환하는 함수
        /// </summary>
        /// <returns>GameRuleSettings</returns>
        public GameRuleSettings GetGameRulePayload()
        {
            return new GameRuleSettings
            {
                mafiaAmount = currentGameRuleSettings.mafiaAmount,
                detectiveAmount = currentGameRuleSettings.detectiveAmount,
            };
        }

        /// <summary>
        /// 내부 참조용 룰을 반환하는 함수
        /// </summary>
        /// <returns>GameRuleSettings</returns>
        public GameRuleSettings GetGameRules()
        {
            return currentGameRuleSettings;
        }

        #endregion
    }
}