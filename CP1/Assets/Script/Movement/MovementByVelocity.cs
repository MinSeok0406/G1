using UnityEngine;

#region RequireComponent
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(Rigidbody2D))]
#endregion
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private MovementByVelocityEvent movementByVelocityEvent;
    private Rigidbody2D rb;

    private void Awake()
    {
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityEventArgs movementByVelocityEventArgs)
    {
        MoveRigidBody(movementByVelocityEventArgs.moveDirection, movementByVelocityEventArgs.moveSpeed);
    }

    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {
        rb.velocity = moveDirection * moveSpeed;
    }
}
