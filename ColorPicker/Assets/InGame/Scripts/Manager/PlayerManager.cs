
using Photon.Pun;
using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerManager : SingletonNetworkBehaviour<PlayerManager>
    {
        private Player myPlayer;
        private LobbyPlayer myLobbyPlayer;

        private LobbyPlayerStaticData cachedStaticData;
        private LobbyPlayerDynamicData cachedDynamicData;

        public Player GetMyPlayer() => myPlayer;
        public void SetMyPlayer(Player player) { myPlayer = player; }

        public LobbyPlayer GetMyLobbyPlayer() => myLobbyPlayer;
        public void SetMyLobbyPlayer(LobbyPlayer lobbyPlayer) { myLobbyPlayer = lobbyPlayer; }

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 정적 플레이어 데이터 설정 함수
        /// </summary>
        public void SetStaticPlayerData(string googleUID, string nickname, int actorId)
        {
            if (string.IsNullOrEmpty(googleUID) || string.IsNullOrEmpty(nickname))
            {
                Debug.LogError("[PlayerManager] SetStaticPlayerData(): 유효하지 않은 입력");
                return;
            }

            cachedStaticData = new LobbyPlayerStaticData
            {
                googleUID = googleUID,
                nickname = nickname,
                actorId = actorId
            };
        }

        /// <summary>
        /// 동적 플레이어 데이터 설정 함수
        /// </summary>
        public void SetDynamicPlayerData(LobbyPlayerDynamicData dynamicData)
        {
            if (cachedDynamicData == null)
            {
                Debug.LogWarning("[PlayerManager] UpdateLobbyPlayerPos(): dynamicData가 초기화되지 않음");
                return;
            }

            cachedDynamicData = dynamicData;
        }

        /// <summary>
        /// LobbyPlayer의 위치 데이터를 갱신하는 함수
        /// </summary>
        public void UpdateLobbyPlayerPos(Vector2 pos)
        {
            cachedDynamicData.posX = pos.x;
            cachedDynamicData.posY = pos.y;
        }

        /// <summary>
        /// 준비 상태를 갱신하는 함수
        /// </summary>
        public void UpdateReadyState(bool isReady)
        {
            if (cachedDynamicData == null)
            {
                Debug.LogWarning("[PlayerManager] UpdateReadyState(): dynamicData가 초기화되지 않음");
                return;
            }

            cachedDynamicData.isReady = isReady;
        }

        /// <summary>
        /// 정적/동적 데이터를 통합하여 LobbyPlayerData로 반환하는 함수
        /// </summary>
        /// <returns>LobbyPlayerData 객체 (static + dynamic 포함)</returns>
        public LobbyPlayerData GetMergedPlayerData()
        {
            if (cachedStaticData == null || cachedDynamicData == null)
            {
                Debug.LogError("[PlayerManager] GetMergedPlayerData(): 캐시 데이터가 완전하지 않음");
                return null;
            }


            return new LobbyPlayerData { staticData = cachedStaticData, dynamicData = cachedDynamicData};
        }

        /// <summary>
        /// 정적/동적 캐시 데이터를 모두 초기화하는 함수
        /// </summary>
        public void ClearCachedData()
        {
            cachedStaticData = null;
            cachedDynamicData = null;

            Debug.Log("[PlayerManager] 캐시 초기화 완료");
        }
    }
}
