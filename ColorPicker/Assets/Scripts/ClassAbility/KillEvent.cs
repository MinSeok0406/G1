
using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class KillEvent : MonoBehaviour
    {
        public event Action<KillEvent, KillEventArgs> OnKill;

        public void CallKillEvent(Player player, float cooldown)
        {
            OnKill?.Invoke(this, new KillEventArgs()
            {
                player = player,
                cooldown = cooldown
            });
        }
    }

    public class KillEventArgs : EventArgs
    {
        public Player player;
        public float cooldown;
    }
}
