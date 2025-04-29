using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

/*#region RequireComponent
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(Rigidbody2D))]
#endregion*/
//[DisallowMultipleComponent]

namespace Minseok
{
    public class MovementByVelocity : MonoBehaviour
    {
        private PlayerControl pc;
        private MovementByVelocityEvent movementByVelocityEvent;
        private Rigidbody2D rb;

        private void Awake()
        {
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            rb = GetComponent<Rigidbody2D>();
            pc = GetComponent<PlayerControl>();
        }

        private void OnEnable()
        {
            movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
        }

        private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityEventArgs movementByVelocityEventArgs)
        {
            //MoveRigidBody(movementByVelocityEventArgs.moveDirection, movementByVelocityEventArgs.moveSpeed);
        }

        private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
        {
            rb.velocity = moveDirection * moveSpeed * Time.unscaledDeltaTime;
            PlayerControl.Instance.State = CreatureState.Moving;
        }
    }
}