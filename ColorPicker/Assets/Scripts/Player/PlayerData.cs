using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [Serializable]
    public class PlayerData 
    {
        public int playerId; // actorNum
        public string playerName;
        public Player player;
        public bool isAlive = true;

        public PlayerData(int playerId, string playerName, Player player, bool isAlive = true)
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.player = player;
            this.isAlive = isAlive;
        }
    }
}
