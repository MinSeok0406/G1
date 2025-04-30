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
        public int playerColor;
        public int playerClass;

        public PlayerData(int playerId, string playerName, int playerColor, int playerClass)
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.playerColor = playerColor;
            this.playerClass = playerClass;
        }
    }
}
