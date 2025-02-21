using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    private List<int> playersID = new List<int>();
    private Dictionary<int, Player> playerDictionary = new Dictionary<int, Player>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void RegisterPlayer(Player player)
    {
        if (!playerDictionary.ContainsKey(GetPlayerID(player)))
        {
            int playerID = GetPlayerID(player);

            playersID.Add(playerID);
            playerDictionary.Add(player.gameObject.GetInstanceID(), player);
        }
    }

    private void SelectClass(PlayerClassType playerClassType, int classAmount)
    {
        int classCount = 0;
        int tryCount = 0;

        if (playerClassType == PlayerClassType.Citizen || classAmount == 0) return;

        while(classCount < classAmount && tryCount < Settings.selectClassMaxTryCount)
        {
            int playerNum = Random.Range(0, playerDictionary.Count);

            if (playerDictionary[playersID[playerNum]].playerClassType == PlayerClassType.Citizen)
            {
                playerDictionary[playersID[playerNum]].playerClassType = playerClassType;
                classCount++;
            }

            tryCount++;
        }
    }

    private int GetPlayerID(Player player)
    {
        return player.gameObject.GetInstanceID();
    } 
}
