using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class CacheDataManager : SingletonMonobehaviour<CacheDataManager>
    {
        private List<LobbyPlayerData> cachedPlayers = new List<LobbyPlayerData>();

        public void SaveAll(List<LobbyPlayerData> source)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Only MasterClient can save cache.");
                return;
            }

            cachedPlayers = new List<LobbyPlayerData>(source);
            Debug.Log($"[CacheDataManager] Cached {cachedPlayers.Count} players.");
        }

        public List<LobbyPlayerData> GetAll()
        {
            return new List<LobbyPlayerData>(cachedPlayers);
        }

        public void Clear()
        {
            cachedPlayers.Clear();
            Debug.Log("[CacheDataManager] Cleared.");
        }

    }
}
