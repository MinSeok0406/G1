using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class HostMigrationHandler
    {
        public void OnHostChanged(Photon.Realtime.Player newMasterClient)
        {
            Debug.Log($"[HostMigration] New Master: {newMasterClient.NickName}");

            if (!PhotonNetwork.IsMasterClient)return; // 안전성 체크: 이 클라이언트가 마스터인지 확인

            // 게임이 이미 시작되었는지 확인
            if (!GameDataManager.Instance.IsGameStarted())
            {
                Debug.Log("[HostMigration] Game not started yet, nothing to resync.");
                return;
            }

            // 2. 필요한 동기화 수행
            SyncCriticalGameData();

            Debug.Log("[HostMigration] Host migration completed.");
        }


        private void SyncCriticalGameData()
        {
            Debug.Log("[HostMigration] Syncing critical player data...");

            GameDataManager.Instance.SyncAllPlayerDataToClients();
        }
    }
}
