using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePlayer : MonoBehaviour
{
    private PlayerControl player;

    private void Awake()
    {
        player = GetComponent<PlayerControl>();
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

    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityEventArgs movementByVelocityEventArgs)
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

    private void SetMovementAnimationParameters(MovementByVelocityEventArgs movementByVelocityEventArgs)
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);

        if (movementByVelocityEventArgs.moveDirection == Google.Protobuf.Protocol.MoveDir.None) return;

        if (movementByVelocityEventArgs.moveDirection == Google.Protobuf.Protocol.MoveDir.Up || movementByVelocityEventArgs.moveDirection == Google.Protobuf.Protocol.MoveDir.Down) return;

        if (movementByVelocityEventArgs.moveDirection == Google.Protobuf.Protocol.MoveDir.Left)
        {
            player.animator.SetBool(Settings.isRight, false);
            player.animator.SetBool(Settings.isLeft, true);
        }
        else
        {
            player.animator.SetBool(Settings.isRight, true);
            player.animator.SetBool(Settings.isLeft, false);
        }
    }

}
