using ColorPicker.InGame;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class AnimatePlayer : MonoBehaviour
    {
        private IdleEvent idleEvent;
        private MovementByVelocityEvent movementByVelocityEvent;
        private DeathEvent deathEvent;
        private Animator animator;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();
            movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
            animator = GetComponent<Animator>();
            deathEvent = GetComponent<DeathEvent>();
        }

        private void OnEnable()
        {
            idleEvent.OnIdle += IdleEvent_OnIdle;

            movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;

            deathEvent.OnDeathEvent += DeathEvent_OnDeathEvent;
        }

        private void OnDisable()
        {
            idleEvent.OnIdle -= IdleEvent_OnIdle;

            movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;

            deathEvent.OnDeathEvent -= DeathEvent_OnDeathEvent;
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

        private void DeathEvent_OnDeathEvent(DeathEvent deathEvent)
        {
            animator.SetBool(Settings.isDead, true);
        }

        private void InitializedAnimationParameters()
        {
            animator.SetBool(Settings.isMoving, false);
            animator.SetBool(Settings.isIdle, false);
        }

        private void SetIdleAnimationParameters()
        {
            animator.SetBool(Settings.isMoving, false);
            animator.SetBool(Settings.isIdle, true);

        }

        private void SetMovementAnimationParameters(MovementByVelocityArgs movementByVelocityEventArgs)
        {
            animator.SetBool(Settings.isMoving, true);
            animator.SetBool(Settings.isIdle, false);

            if (movementByVelocityEventArgs.moveDirection.x < 0)
            {
                animator.SetBool(Settings.isRight, false);
                animator.SetBool(Settings.isLeft, true);
            }
            else if (movementByVelocityEventArgs.moveDirection.x > 0)
            {
                animator.SetBool(Settings.isRight, true);
                animator.SetBool(Settings.isLeft, false);
            }
        }

    }
}