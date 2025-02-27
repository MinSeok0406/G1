﻿using UnityEngine;

#region RequireComponent
[RequireComponent(typeof(Player))]
#endregion
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    private Player player;
    private float moveSpeed;

    private void Awake()
    {
        player = GetComponent<Player>();

        moveSpeed = Settings.playerMoveSpeed;
    }

    private void Update()
    {
        MovementInput();
    }

    private void MovementInput()
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
            player.movementByVelocityEvent.CallMovementByVelocity(direction, moveSpeed);
        }
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }
}

