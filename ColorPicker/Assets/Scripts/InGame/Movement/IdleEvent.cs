using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    [DisallowMultipleComponent]
    public class IdleEvent : MonoBehaviour
    {
        public event Action<IdleEvent> OnIdle;

        public void CallIdleEvent()
        {
            OnIdle?.Invoke(this);
        }
    }

}
