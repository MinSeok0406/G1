using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private MovementByVelocityEvent movementByVelocityEvent;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
    }

    private void OnEnable()
    {
        movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void OnDisable()
    {
        movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        MoveRigidBody(movementByVelocityArgs.moveDirection, movementByVelocityArgs.moveSpeed);
    }

    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {
        rigidBody2D.velocity = moveDirection * moveSpeed;
    }
}
