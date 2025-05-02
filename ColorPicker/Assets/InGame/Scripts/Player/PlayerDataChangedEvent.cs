

using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerDataChangedEvent : MonoBehaviour
    {
        public event Action<PlayerDataChangedEvent, PlayerDataChangedEventArgs> OnPlayerDataChangeEvent;

        public void CallPlayerDataChanageEvent(PlayerData playerData)
        {
            OnPlayerDataChangeEvent?.Invoke(this, new PlayerDataChangedEventArgs() { playerData = playerData });
        }
    }

    public class PlayerDataChangedEventArgs : EventArgs
    {
        public PlayerData playerData;
    }
}