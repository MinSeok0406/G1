using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class DeathEvent : MonoBehaviour
    {
        public event Action<DeathEvent> OnDeathEvent;

        public void CallDeathEvent()
        {
            OnDeathEvent?.Invoke(this);
        }
    }
}
