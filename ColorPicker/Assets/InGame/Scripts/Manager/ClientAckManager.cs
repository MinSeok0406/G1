using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class ClientAckManager : SingletonNetworkBehaviour<ClientAckManager>
    {
        private List<int> ackedList = new List<int>();
        private Coroutine waitCoroutine;
        private Action onAllAckReceived;

        private float ackRetryInterval = 1.0f;
        private bool isWaiting = false;

        public void WaitForAllClients(Action callback)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Debug.Log("[ClientAckManager] ACK 대기 시작");

            ackedList.Clear();
            onAllAckReceived = callback;
            isWaiting = true;

            if (waitCoroutine != null)
                StopCoroutine(waitCoroutine);

            waitCoroutine = StartCoroutine(AckRequestLoop());
        }

        private IEnumerator AckRequestLoop()
        {
            while (isWaiting)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (!ackedList.Contains(player.ActorNumber))
                    {
                        photonView.RPC(nameof(RPC_RequestAck), player);
                    }
                }

                yield return new WaitForSeconds(ackRetryInterval);
            }
        }

        [PunRPC]
        private void RPC_RequestAck()
        {
            photonView.RPC(nameof(RPC_SendAck), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [PunRPC]
        private void RPC_SendAck(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient || !isWaiting) return;

            if (!ackedList.Contains(actorId))
            {
                ackedList.Add(actorId);
                Debug.Log($"[ClientAckManager] ACK 수신: {actorId}");

                StartCoroutine(CheckAckCompletion());
            }
        }

        private IEnumerator CheckAckCompletion()
        {
            var currentPlayers = PhotonNetwork.PlayerList;
            bool allAcked = true;

            foreach (var player in currentPlayers)
            {
                if (!ackedList.Contains(player.ActorNumber))
                {
                    allAcked = false;
                    break;
                }
            }

            if (allAcked)
            {
                Debug.Log("[ClientAckManager] 모든 클라이언트 ACK 수신 완료");
                isWaiting = false;
                if (waitCoroutine != null)
                {
                    StopCoroutine(waitCoroutine);
                    waitCoroutine = null;
                }

                yield return new WaitForSeconds(0.5f);

                onAllAckReceived?.Invoke();
            }
        }

        public void ResetWait()
        {
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            ackedList.Clear();
            onAllAckReceived = null;
            isWaiting = false;
        }
    }
}