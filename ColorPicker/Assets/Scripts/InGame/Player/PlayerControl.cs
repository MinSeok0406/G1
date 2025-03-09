using Mirror;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerControl : NetworkBehaviour
    {
        [SyncVar] private float moveSpeed;

        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();

            moveSpeed = Settings.moveSpeed;
        }

        private void FixedUpdate()
        {
            if (!isOwned) return;

            MoveInput();
        }

        private void MoveInput()
        {
            float horizontalMovement = Input.GetAxisRaw("Horizontal");
            float verticalMovement = Input.GetAxisRaw("Vertical");

            Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

            if (horizontalMovement != 0f && verticalMovement != 0f)
            {
                direction = direction.normalized;
            }

            if (direction != Vector2.zero)
            {
                player.movementByVelocityEvent.CallMovementByVElocityEvent(direction, moveSpeed);
            }
            else
            {
                player.idleEvent.CallIdleEvent();
            }
        }
    }
}

