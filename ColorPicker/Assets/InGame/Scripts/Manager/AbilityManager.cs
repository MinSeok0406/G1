using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class AbilityManager : SingletonNetworkBehaviour<AbilityManager>
    {
        private Dictionary<int, float> cooldownEndTimes = new Dictionary<int, float>();
        private float defaultCooldownTime => Settings.defaultCooldown;

        #region Cooldown Handling

        /// <summary>
        /// 현재 플레이어가 쿨타임 중인지 확인하는 함수
        /// </summary>
        public bool IsOnCooldown(int viewID)
        {
            return cooldownEndTimes.TryGetValue(viewID, out float endTime) && Time.time < endTime;
        }

        /// <summary>
        /// [호스트 전용] 스킬을 사용했을 때 해당 플레이어의 쿨타임을 시작하는 함수
        /// </summary>
        /// <param name="viewID"> Player View ID </param>
        /// <param name="skillCooldownDuration"> 클래스별 쿨타임 </param>
        public void StartCooldown(int viewID, float skillCooldownDuration)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float duration = skillCooldownDuration;
            cooldownEndTimes[viewID] = Time.time + duration;
        }

        /// <summary>
        /// [호스트 전용]남은 쿨타임을 초 단위로 반환하는 함수
        /// 재접속 하는 유저에게 남은 쿨타임을 반환해주기 위해서 사용
        /// </summary>
        /// <param name="viewID"> view ID </param>
        /// <returns> 남은 쿨타임 시간 반환 </returns>
        public float GetRemainingCooldown(int viewID)
        {
            return cooldownEndTimes.TryGetValue(viewID, out float endTime)? Mathf.Max(0f, endTime - Time.time) : 0f;
        }

        #endregion

        #region Kill Event

        public void TryRequestKill(int targetViewID, int killerViewID)
        {
            photonView.RPC(nameof(RPC_RequestKill), RpcTarget.MasterClient, targetViewID, killerViewID);
        }

        [PunRPC]
        private void RPC_RequestKill(int targetViewID, int killerViewID)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameDataManager.Instance.TryGetUIDByViewID(targetViewID, out string targetID);
            GameDataManager.Instance.TryGetUIDByViewID(killerViewID, out string killerID);

            if (!IsKillValid(targetID, killerID))
            {   
                return;
            }

            ApplyKill(killerID, targetID);
        }

        private bool IsKillValid(string targetID, string killerID)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(killerID, out var killerData) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(targetID, out var targetData))
                return false;

            if (killerData.classType != (int)PlayerClassType.mafia) return false;
            if (targetData.classType == (int)PlayerClassType.ghost) return false;

            return true;
        }

        private void ApplyKill(string targetID, string killerID)
        {
            if (!GameDataManager.Instance.TryGetPrivatePlayerData(targetID, out var targetData)) return;
            
            targetData.classType = (int)PlayerClassType.ghost;

            GameDataManager.Instance.UpdatePrivatePlayerData(targetData);

            GameDataManager.Instance.TryGetViewIDByUID(targetID, out int targetViewID);

            photonView.RPC(nameof(RPC_ConfirmKillResult), RpcTarget.All, targetViewID);

            StartCooldown(targetViewID, Settings.defaultCooldown);
        }

        [PunRPC]
        private void RPC_ConfirmKillResult(int targetViewID)
        {
            PhotonView view = PhotonView.Find(targetViewID);

            if (view != null)
            {
                Player player = view.GetComponent<Player>();
                //플레이어 사망 이벤트 발생 추가 *****
            }
        }

        #endregion
    }
}