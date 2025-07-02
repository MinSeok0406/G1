using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class AESKeyManager : SingletonNetworkBehaviour<AESKeyManager>
    {
        private byte[] aesKeyBytes; // AES 암호화는 직접적으로 이진데이터를 다루기 때문에 byte로 저장
        private bool keyDistributed = false;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// AES 키를 생성하고 모든 클라이언트에게 평문으로 전송
        /// </summary>
        public void GenerateAndDistributeAESKey()
        {
            if (!PhotonNetwork.IsMasterClient || keyDistributed) return;

            aesKeyBytes = GenerateRandomKeyBytes(32); // 32byte -> 256-bit key
            string keyBase64 = Convert.ToBase64String(aesKeyBytes); //이진데이터 문자열로 인코딩

            photonView.RPC(nameof(ReceiveAESKey), RpcTarget.Others, keyBase64);

            keyDistributed = true;
            Debug.Log("[AESKeyManager] AES 키가 모든 클라이언트에 평문으로 전송됨");
        }

        /// <summary>
        /// 새로 접속한 클라이언트에게도 AES 키 전달
        /// </summary>
        public void DistributeKeyToNewClient(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient || !keyDistributed || aesKeyBytes == null) return;

            string keyBase64 = Convert.ToBase64String(aesKeyBytes);
            var target = PhotonNetwork.CurrentRoom.GetPlayer(actorId);
            if (target != null)
                photonView.RPC(nameof(ReceiveAESKey), target, keyBase64);

            Debug.Log($"[AESKeyManager] 새 클라이언트 {actorId} 에게 AES 키 전송 완료");
        }

        [PunRPC]
        private void ReceiveAESKey(string keyBase64)
        {
            aesKeyBytes = Convert.FromBase64String(keyBase64);
            Debug.Log("[AESKeyManager] AES 키 수신 완료");
        }

        public byte[] GetAESKeyBytes() => aesKeyBytes;

        public bool HasKey() => aesKeyBytes != null && aesKeyBytes.Length > 0;

        public void ClearKey()
        {
            if (aesKeyBytes != null)
                Array.Clear(aesKeyBytes, 0, aesKeyBytes.Length);
            aesKeyBytes = null;
        }

        private byte[] GenerateRandomKeyBytes(int length)
        {
            byte[] key = new byte[length]; 
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key); // 난수채우기
            }
            return key;
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient || !keyDistributed) return;
            Debug.Log($"[AESKeyManager] 새로운 플레이어 입장 감지, 키 전송 중 : {newPlayer.NickName}");
            DistributeKeyToNewClient(newPlayer.ActorNumber);
        }
    }
}
