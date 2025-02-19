using System;
using UnityEngine;

[DisallowMultipleComponent]
public class MovementByVelocityEvent : MonoBehaviour
{
    public event Action<MovementByVelocityEvent, MovementByVelocityEventArgs> OnMovementByVelocity;

    public void CallMovementByVelocity(Vector2 moveDirection, float moveSpeed)
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
    public Vector2 moveDirection;
    public float moveSpeed;
}


