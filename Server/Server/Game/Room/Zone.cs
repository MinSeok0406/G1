﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Zone
    {
        public int IndexY {  get; private set; }
        public int IndexX {  get; private set; }

        public HashSet<Player> Players { get; set;  } = new HashSet<Player>();

        public Zone(int y, int x)
        {
            IndexY = y; 
            IndexX = x;
        }

        public void Remove(GameObject gameObject)
        {
            //GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            /*switch (type)
            {
                case GameObjectType.Player:
                    Players.Remove((Player)gameObject);
                    break;
            }*/
        }

        public Player FindOnePlayer(Func<Player, bool> condition)
        {
            foreach (Player player in Players)
            {
                if (condition.Invoke(player)) 
                    return player;
            }

            return null;
        }

        public List<Player> FindAllPlayers(Func<Player, bool> condition)
        {
            List<Player> findList = new List<Player>();

            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    findList.Add(player);
            }

            return null;
        }
    }
}
