using Photon.Pun;
using UnityEngine;

//namespace ColorPicker.InGame
//{
//    public class PacketHandler : SingletonNetworkBehaviour<PacketHandler>
//    {
//        protected override void Awake()
//        {
//            base.Awake();

//            DontDestroyOnLoad(gameObject);
//        }

//        // 서버가 플레이어 데이터를 유지하고 함수를 통해 모든 플레이어에게 전달
//        public void SendPlayerData(PlayerData playerData)
//        {
//            photonView.RPC("ReceivePlayerData", RpcTarget.AllBuffered, playerData.playerId, playerData.playerName, playerData.playerColor, playerData.playerClass);
//        }

//        [PunRPC]
//        public void ReceivePlayerData(int playerId, string playerName,int playerColor, int playerClass)
//        {
//            PlayerData playerData = new PlayerData(playerId, playerName, playerColor, playerClass);

//            NetworkManager.Instance.AddOrUpdatePlayerData(playerData);
//        }
//    }
//}
