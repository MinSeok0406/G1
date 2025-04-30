using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class AbilityManager : SingletonNetworkBehaviour<AbilityManager>
    {
        private Dictionary<int, float> playerCooldowns = new Dictionary<int, float>(); // 플레이어별 쿨타임
        private Dictionary<int, bool> isOnCooldown = new Dictionary<int, bool>(); // 플레이어별 쿨타임 상태

        private float defaultCooldownTime = Settings.defaultCooldown; // 기본 쿨타임 설정 *추후 방 데이터를 가져와서 적용하도록 수정예정

        #region about Cooldown
        // 플레이어의 쿨타임을 시작하는 함수
        public void StartCooldown(int playerId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 플레이어의 쿨타임을 설정 (if.존재하지 않으면 새로 추가)
            if (!playerCooldowns.ContainsKey(playerId))
                playerCooldowns[playerId] = 0f;

            if (!isOnCooldown.ContainsKey(playerId))
                isOnCooldown[playerId] = false;

            // 쿨타임 설정
            playerCooldowns[playerId] = defaultCooldownTime;
            isOnCooldown[playerId] = true;

            // 쿨타임 코루틴 시작
            StartCoroutine(CooldownRoutine(playerId));
        }

        // 쿨타임 진행을 위한 코루틴
        private IEnumerator CooldownRoutine(int playerId)
        {
            Photon.Realtime.Player targetPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == playerId);

            // playerId와 매칭되는 플레이어가 없으면 종료
            if (targetPlayer == null)
            {
                Debug.LogError($"Player with ID {playerId} not found.");
                yield break;
            }

            // 쿨타임 동안 대기
            while (playerCooldowns[playerId] > 0)
            {
                playerCooldowns[playerId] -= Time.deltaTime;

                // 쿨타임 UI 업데이트
                photonView.RPC("RPC_UpdatePlayerCooldown", targetPlayer, playerCooldowns[playerId]);
                yield return null;
            }

            isOnCooldown[playerId] = false;
            photonView.RPC("RPC_UpdatePlayerCooldown", targetPlayer, 0f); // 쿨타임 종료

            // 버튼 재활성화 로직 추가 예정*************
        }

        // 각 클라이언트의 쿨타임 상태를 업데이트
        [PunRPC]
        public void RPC_UpdatePlayerCooldown(float remainingTime)
        {
            // UI에 쿨타임 시간 업데이트
            UIManager.Instance.UpdatePlayerCooldownUI(remainingTime);
        }

        // 플레이어의 쿨타임 상태 확인
        public bool IsOnCooldown(int playerId)
        {
            // player가 딕셔너리 안에 존재하고, 남은 쿨타임이 0초가 아닐 때 true
            return isOnCooldown.ContainsKey(playerId) && isOnCooldown[playerId]; 
        }
        #endregion

        #region Kill Event
        public void TryPlayerKill(int targetPlayerId)
        {
            photonView.RPC("RPC_RequestKill", RpcTarget.MasterClient, targetPlayerId);
        }

        [PunRPC]
        private void RPC_RequestKill(int targetId, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int killerId = info.Sender.ActorNumber;

            if (!NetworkManager.Instance.TryGetPlayerData(killerId, out PlayerData killerData) ||
                !NetworkManager.Instance.TryGetPlayerData(targetId, out PlayerData targetData)) return;

            if (targetData.playerClass == (int)PlayerClassType.ghost) return; // 이미 죽은 플레이어라면 실패
            if (killerData.playerClass != (int)PlayerClassType.mafia) return; // 마피아가 아닌 플레이어가 킬 하려고 할 시 실패

            // 킬 확정
            targetData.playerClass = (int)PlayerClassType.ghost;

            Photon.Realtime.Player killerRpcPlayer = PhotonNetwork.CurrentRoom.GetPlayer(killerId);

            photonView.RPC("RPC_TeleportTo", killerRpcPlayer, killerId, targetId);

            // 킬 결과 전체 동기화
            photonView.RPC("RPC_ConfirmKill", RpcTarget.All, targetId, killerId);

            StartCooldown(killerId);

            // PlayerData 동기화
            NetworkManager.Instance.SyncPlayerData();
        }

        [PunRPC]
        private void RPC_TeleportTo(int killerId, int targetId)
        {
            Player killerPlayer = NetworkManager.Instance.GetPlayerObject(killerId);
            Player targetPlayer = NetworkManager.Instance.GetPlayerObject(targetId);

            killerPlayer.transform.position = targetPlayer.transform.position;
        }

        [PunRPC]
        private void RPC_ConfirmKill(int targetId, int killerId)
        {
            if (NetworkManager.Instance.TryGetPlayerData(targetId, out PlayerData targetData))
            {
                targetData.playerClass = (int)PlayerClassType.ghost;
                NetworkManager.Instance.GetPlayerObject(targetId).deathEvent.CallDeathEvent();
            }
            else { return; } // Kill 실패
        }

        #endregion
    }
}