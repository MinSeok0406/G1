using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class CharacterCustomizeManager : SingletonNetworkBehaviour<CharacterCustomizeManager>
    {
        public void RequestCustomizationChange(PlayerCustomizationData newCustomization)
        {
            int viewID = PlayerManager.Instance.GetMyPlayer().photonView.ViewID;

            string json = JsonUtility.ToJson(newCustomization);
            photonView.RPC(nameof(RPC_HandleCustomizationRequest), RpcTarget.MasterClient, viewID, json);
        }

        /// <summary>
        /// 로컬 플레이어의 커스터마이즈를 변경하고 전체 동기화 요청
        /// </summary>
        [PunRPC]
        private void RPC_HandleCustomizationRequest(int viewID, string json)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            PlayerCustomizationData newData = JsonUtility.FromJson<PlayerCustomizationData>(json);

            GameDataManager.Instance.TryGetUIDByViewID(viewID, out string uid);

            GameDataManager.Instance.TryGetPublicPlayerData(uid, out var data);
            data.customizationData = newData;
            GameDataManager.Instance.UpdatePublicPlayerData(data);

            photonView.RPC(nameof(RPC_ApplyCustomizationToAll), RpcTarget.All, viewID, newData);
        }

        [PunRPC]
        private void RPC_ApplyCustomizationToAll(int viewID, string json)
        {
            Player player = PhotonView.Find(viewID).gameObject.GetComponent<Player>();

            PlayerCustomizationData newData = JsonUtility.FromJson<PlayerCustomizationData>(json);

            player.GetComponent<PlayerCustomization>().ApplyCustomization(newData);
        }

        //private Dictionary<string, PlayerCustomizationData> customizeDataDict = new Dictionary<string, PlayerCustomizationData>();
        //private HashSet<int> usedColorIds = new HashSet<int>();

        ///// <summary>
        ///// 특정 UID의 커스터마이즈 데이터를 설정(업데이트).
        ///// </summary>
        //public void SetCustomization(string uid, PlayerCustomizationData data)
        //{
        //    customizeDataDict[uid] = data;
        //}

        ///// <summary>
        ///// 특정 UID의 커스터마이즈 데이터를 가져오는 함수.
        ///// </summary>
        //public PlayerCustomizationData GetCustomization(string uid)
        //{
        //    customizeDataDict.TryGetValue(uid, out var data);
        //    return data;
        //}

        ///// <summary>
        ///// 모든 유저의 커스터마이즈 데이터를 가져오는 함수
        ///// </summary>
        //public Dictionary<string, PlayerCustomizationData> GetAllCustomizationData()
        //{
        //    return new Dictionary<string, PlayerCustomizationData>(customizeDataDict);
        //}

        ///// <summary>
        ///// 저장된 모든 데이터를 초기화
        ///// </summary>
        //public void ClearAllCustomization()
        //{
        //    customizeDataDict.Clear();
        //}

        //[PunRPC] 
        //public void RequestCustomizationUpdate(string uid, PlayerCustomizationData data)
        //{
        //    if (!PhotonNetwork.IsMasterClient) return;

        //    photonView.RPC(nameof(ApplyCustomizationToAll), RpcTarget.All, uid, JsonUtility.ToJson(data));
        //}

        ///// <summary>
        ///// 모든 클라이언트에게 커스터마이징 적용
        ///// </summary>
        //[PunRPC]
        //public void ApplyCustomizationToAll(string uid, string jsonData)
        //{
        //    var data = JsonUtility.FromJson<PlayerCustomizationData>(jsonData);
        //    SetCustomization(uid, data);

        //    GameDataManager.Instance.TryGetViewIDByUID(uid, out int viewID);

        //    Player player = PhotonView.Find(viewID).GetComponent<Player>();
        //    player?.GetComponent<PlayerCustomization>()?.ApplyCustomization(data);
        //}

        ///// <summary>
        ///// 클라이언트에서 요청 보낼 때 호출하는 편의 함수
        ///// </summary>
        //public void RequestUpdateFromClient(string uid, PlayerCustomizationData data)
        //{
        //    photonView.RPC(nameof(RequestCustomizationUpdate), RpcTarget.MasterClient, uid, data);
        //}


        ///// <summary>
        ///// 플레이어 입장 시 커스터마이즈 자동 설정
        ///// </summary>
        //public void OnPlayerJoin(string uid)
        //{
        //    var newData = new PlayerCustomizationData
        //    {
        //        customColorId = (int)AssignAvailableCustomColor(),
        //        hatId = 0
        //    };
        //    SetCustomization(uid, newData);
        //}

        ///// <summary>
        ///// 플레이어 퇴장 시 커스터마이즈 제거 및 색상 반환
        ///// </summary>
        //public void OnPlayerLeave(string uid)
        //{
        //    if (customizeDataDict.TryGetValue(uid, out var data))
        //    {
        //        ReleaseColor(data.customColorId);
        //        customizeDataDict.Remove(uid);
        //    }
        //}

        ///// <summary>
        ///// 사용되지 않은 커스터마이즈 색상 중 하나를 랜덤으로 반환하고 사용 처리함
        ///// </summary>
        //public CustomizationColor AssignAvailableCustomColor()
        //{
        //    // 모든 Enum 값을 가져옴
        //    var allColors = Enum.GetValues(typeof(CustomizationColor)).Cast<CustomizationColor>().ToList();

        //    // 사용 가능한 색상 추림
        //    var available = allColors
        //        .Where(color => !usedColorIds.Contains((int)color))
        //        .ToList();

        //    if (available.Count == 0)
        //    {
        //        Debug.LogWarning("[CustomizeManager] 사용 가능한 색상이 없습니다. 기본값 반환.");
        //        return CustomizationColor.White; // fallback 또는 다른 default
        //    }

        //    // 랜덤 선택
        //    var selected = available[UnityEngine.Random.Range(0, available.Count)];
        //    usedColorIds.Add((int)selected);
        //    return selected;
        //}

        ///// <summary>
        ///// 플레이어가 나가면 색상 반환
        ///// </summary>
        //public void ReleaseColor(int colorId)
        //{
        //    usedColorIds.Remove(colorId);
        //}


        //public void InitializeCustomizationForPlayer(string uid)
        //{
        //    if (!PhotonNetwork.IsMasterClient) return;

        //    // 커스터마이징 데이터 생성
        //    var newData = new PlayerCustomizationData
        //    {
        //        customColorId = (int)AssignAvailableCustomColor(),
        //        hatId = 0
        //    };

        //    // 내부 딕셔너리에 저장
        //    SetCustomization(uid, newData);

        //    // GameDataManager의 PlayerData에도 저장
        //    if (GameDataManager.Instance.TryGetPlayerData(uid, out var playerData))
        //    {
        //        playerData.customizationData = newData;
        //        GameDataManager.Instance.UpdatePlayerData(playerData);
        //    }

        //    // 클라이언트 전체에 커스터마이징 반영
        //    photonView.RPC(nameof(ApplyCustomizationToAll), RpcTarget.All, uid, JsonUtility.ToJson(newData));
        //}
    }
}

