using Google.Protobuf.Protocol;
using System;
using UnityEngine;

//[DisallowMultipleComponent]

namespace Minseok
{
    public class MovementByVelocityEvent : MonoBehaviour
    {
        public event Action<MovementByVelocityEvent, MovementByVelocityEventArgs> OnMovementByVelocity;

        public void CallMovementByVelocity(MoveDir moveDirection, float moveSpeed)
        {
            OnMovementByVelocity?.Invoke(this, new MovementByVelocityEventArgs()
            {
                moveDirection = moveDirection,
                moveSpeed = moveSpeed
            });
        }
    }

    public class MovementByVelocityEventArgs : EventArgs
    {
        public MoveDir moveDirection;
        public float moveSpeed;
    }
}