
using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class KillEvent : MonoBehaviour
    {
        public event Action<KillEvent, KillEventArgs> OnKill;

        public void CallKillEvent(int playerId)
        {
            OnKill?.Invoke(this, new KillEventArgs()
            {
                playerId = playerId,
            });
        }
    }

    public class KillEventArgs : EventArgs
    {
        public int playerId;
    }
}
