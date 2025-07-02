using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;

namespace ColorPicker.InGame
{
    public class NetworkManager : SingletonNetworkBehaviour<NetworkManager>
    {
        private HostMigrationHandler hostMigration; // 마스터 클라이언트 승계시 사용하는 클래스

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            hostMigration = new HostMigrationHandler();
        }

        private void Start()
        {
            CheckAndInitializeRoomJoin();
        }

        private void CheckAndInitializeRoomJoin()
        {
            StartCoroutine(VerifyRoomJoinAndSetup());
        }

        private IEnumerator VerifyRoomJoinAndSetup()
        {
            float connectStartTime = Time.time;

            while (Time.time < connectStartTime + Settings.RoomJoinTimeoutSeconds)
            {
                if (PhotonNetwork.InRoom)
                {
                    InitializeAfterJoin();
                    yield break;
                }

                yield return new WaitForSeconds(Settings.RoomJoinRetryIntervalSeconds);
            }

            HandleRoomJoinTimeout();
        }

        /// <summary>
        /// 자신의 GoogleUID와 ActorNumber를 마스터 클라이언트에게 전달하여 등록을 요청.
        /// </summary>
        private void InitializeAfterJoin()
        {
            // GooglePlay에서 Id를 가져옴 // **** 추후 데디케이트 서버에서 받아오도록 설계 **** 
            //sting googleUID = GooglePlayService.GetUserId(); 
            string googleUID = System.Guid.NewGuid().ToString(); // 임시코드
            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC(nameof(TryRegisterPlayer), RpcTarget.MasterClient, googleUID, actorId);           
        }

        private void HandleRoomJoinTimeout()
        {
            PhotonNetwork.LeaveRoom();

            PhotonNetwork.LoadLevel(Settings.testMainScene);
        }

        /// <summary>
        /// 호스트 마이그레이션 핸들러를 통해 호스트 위임시 동작.
        /// </summary>
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            hostMigration.OnHostChanged(newMasterClient); // 호스트 변경
        }

        /// <summary>
        /// [RPC][호스트 전용] 플레이어 등록 또는 재접속 처리를 수행하는 함수
        /// </summary>
        /// <param name="googleUID"> 플레이어 고유 식별자(Google UID) </param>
        /// <param name="actorId"> ActorNumber </param>
        [PunRPC]
        private void TryRegisterPlayer(string googleUID, int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            bool isRejoin = GameDataManager.Instance.TryGetPublicPlayerData(googleUID, out var data);

            if (isRejoin)
            {
                HandleRejoinPlayer(data, actorId);
            }
            else
            {
                SpawnPlayer(googleUID, actorId);
            }
        }

        /// <summary>
        /// [호스트 전용] 재접속한 플레이어의 데이터를 갱신하고 기존 오브젝트의 소유권을 새 Actor에게 이전하는 함수
        /// </summary>
        /// <param name="publicData">재접속한 플레이어의 PublicPlayerData</param>
        /// <param name="newActorId">재접속 시점의 새로운 ActorNumber</param>
        private void HandleRejoinPlayer(PublicPlayerData publicData, int newActorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Debug.Log($"[NetworkManager] 재접속 처리 : {publicData.googleUID}");

            publicData.currentActorId = newActorId;
            GameDataManager.Instance.UpdatePublicPlayerData(publicData);

            if (GameDataManager.Instance.TryGetPrivatePlayerData(publicData.googleUID, out var privateData))
            {
                privateData.currentActorId = newActorId;
                GameDataManager.Instance.UpdatePrivatePlayerData(privateData);
            }

            ReassignOwnershipToPlayer(publicData.googleUID, newActorId);
        }

        /// <summary>
        /// 새로 입장한 플레이어의 오브젝트를 스폰하고 ViewID 포함 데이터를 등록하는 함수
        /// </summary>
        /// <param name="googleUID"></param>
        public void SpawnPlayer(string googleUID, int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            string prefabName = GameResources.Instance.playerPrefab.name;
            GameObject playerObj = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
            Player player = playerObj.GetComponent<Player>();
            int viewID = player.photonView.ViewID;

            GameDataManager.Instance.CreateNewPlayerData(googleUID, actorId, viewID);

            ReassignOwnershipToPlayer(googleUID, actorId);
        }

        /// <summary>
        /// 특정 UID를 가진 플레이어의 오브젝트에 대해 소유권을 새 Actor에게 이전하는 함수
        /// 내부적으로 ViewID를 기반으로 매핑된 객체에서 PhotonView를 조회해 소유권 이전 수행.
        /// </summary>
        /// <param name="googleUID">플레이어의 UID</param>
        /// <param name="newActorId">이전할 대상의 ActorNumber</param>
        public void ReassignOwnershipToPlayer(string googleUID, int newActorId)
        {
            if (!GameDataManager.Instance.TryGetViewIDByUID(googleUID, out int viewID)) return;

            Player player = PhotonView.Find(viewID).gameObject.GetComponent<Player>();

            if (player.photonView.OwnerActorNr != newActorId)
            {
                TransferOwnershipSafe(player.photonView, newActorId);
                Debug.Log($"[Ownership] {googleUID} 소유권 이전 완료 → Actor {newActorId}");
            }
        }

        /// <summary>
        /// 소유권을 이전하는 함수
        /// </summary>
        /// <param name="view"></param>
        /// <param name="newActorId"></param>
        public void TransferOwnershipSafe(PhotonView view, int newActorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            view.TransferOwnership(newActorId);
            Debug.Log($"[Ownership] 소유권을 {newActorId} 에게 이전함");
        }
    }
}
