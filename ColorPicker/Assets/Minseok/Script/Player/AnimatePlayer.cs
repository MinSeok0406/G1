using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class AnimatePlayer : MonoBehaviour
    {
        private PlayerControl player;

        private void Awake()
        {
            player = GetComponent<PlayerControl>();
        }

        private void OnEnable()
        {
            //player.idleEvent.OnIdle += IdleEvent_OnIdle;

            //player.movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
        }

        private void OnDisable()
        {
            //player.idleEvent.OnIdle -= IdleEvent_OnIdle;

            //player.movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;
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

            if (movementByVelocityEventArgs.moveDirection == MoveDir.None) return;

            if (movementByVelocityEventArgs.moveDirection == MoveDir.Up || movementByVelocityEventArgs.moveDirection == MoveDir.Down) return;

            if (movementByVelocityEventArgs.moveDirection == MoveDir.Left)
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
}