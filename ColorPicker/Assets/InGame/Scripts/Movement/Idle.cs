using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    #region RequireComponet
    [RequireComponent(typeof(IdleEvent))]
    [RequireComponent(typeof(Rigidbody2D))]
    #endregion
    [DisallowMultipleComponent]
    public class Idle : MonoBehaviour
    {
        private Rigidbody2D rigidBody2D;
        private IdleEvent idleEvent;

        private void Awake()
        {
            rigidBody2D = GetComponent<Rigidbody2D>();
            idleEvent = GetComponent<IdleEvent>();
        }

        private void OnEnable()
        {
            idleEvent.OnIdle += IdleEvent_OnIdle;
        }

        private void OnDisable()
        {
            idleEvent.OnIdle -= IdleEvent_OnIdle;
        }

        private void IdleEvent_OnIdle(IdleEvent idleEvent)
        {
            MoveRigidBody();
        }

        private void MoveRigidBody()
        {
            rigidBody2D.velocity = Vector2.zero;
        }
    }
}
