using ColorPicker.Server;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameSystem : SingletonNetworkBehaviour<GameSystem>
    {
        [HideInInspector] public Dictionary<uint, Player> playerDcitionary = new Dictionary<uint, Player>();
        [HideInInspector] public List<Player> players = new List<Player>();
        [HideInInspector] public List<uint> readyPlayer = new List<uint>();

        protected override void Awake()
        {
            if (isServer)
            {
                base.Awake();
            }
        }

        public void AddPlayer(uint netID,Player player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }

            if (!playerDcitionary.ContainsKey(netID))
            {
                playerDcitionary.Add(netID, player);
            }
        }

        public void AddReadyPlayer(Player player)
        {
            uint PlayerID = player.netId;

            if (!readyPlayer.Contains(PlayerID))
            {
                readyPlayer.Add(PlayerID);
            }
            else
            {
                readyPlayer.Remove(PlayerID);
            }

            Debug.Log($"{readyPlayer.Count}/{players.Count}");
        }

        public void GameStart()
        {
            if (isServer)
            {
                RoomManager.singleton.ServerChangeScene(Settings.inGameSceneName);
            }
        }

        public List<Player> GetPlayerList()
        {
            return players;
        }

        public bool CheckAllPlayerReady()
        {
            var manager = NetworkManager.singleton as RoomManager;
            if (manager.roomSlots.Count == readyPlayer.Count)
            {
                return true;
            }

            return false;
        }

        //private IEnumerator GameReady()
        //{
        //    var manager = NetworkManager.singleton as RoomManager;
        //    while (manager.roomSlots.Count != readyPlayer.Count)
        //    {
        //        yield return null;
        //    }

        //    //i = 2 추후 방세팅에서 maifa amount 로 값 변경 
        //    for (int i = 0; i < 2; i++)
        //    {
        //        var player = players[Random.Range(0, players.Count)];

        //        if(player.playerClassType == PlayerClassType.Citizen)
        //        {
        //            player.playerClassType = PlayerClassType.Mafia;
        //            Debug.Log(player.name);
        //        }
        //        else
        //        {
        //            i--;
        //        }
        //    }
        //}


    }
}
