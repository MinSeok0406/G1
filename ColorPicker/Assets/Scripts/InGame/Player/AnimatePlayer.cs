using ColorPicker.InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePlayer : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.idleEvent.OnIdle += IdleEvent_OnIdle;

        player.movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void OnDisable()
    {
        player.idleEvent.OnIdle -= IdleEvent_OnIdle;

        player.movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        InitializedAnimationParameters();
        SetIdleAnimationParameters();
    }

    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityEventArgs)
    {
        InitializedAnimationParameters();
        SetMovementAnimationParameters(movementByVelocityEventArgs);
    }

    private void InitializedAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);
    }

    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);

    }

    private void SetMovementAnimationParameters(MovementByVelocityArgs movementByVelocityEventArgs)
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);

        if (movementByVelocityEventArgs.moveDirection.x < 0)
        {
            player.animator.SetBool(Settings.isRight, false);
            player.animator.SetBool(Settings.isLeft, true);
        }
        else if(movementByVelocityEventArgs.moveDirection.x > 0)
        {
            player.animator.SetBool(Settings.isRight, true);
            player.animator.SetBool(Settings.isLeft, false);
        }
    }

}
