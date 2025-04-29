using System;
using UnityEngine;

//[DisallowMultipleComponent]

namespace Minseok
{
    public class IdleEvent : MonoBehaviour
    {
        public event Action<IdleEvent> OnIdle;

        public void CallIdleEvent()
        {
            OnIdle?.Invoke(this);
        }
    }
}