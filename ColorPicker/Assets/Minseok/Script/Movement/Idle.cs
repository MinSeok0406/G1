using UnityEngine;

/*#region RequrieComponent
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Rigidbody2D))]
#endregion*/
//[DisallowMultipleComponent]

namespace Minseok
{
    public class Idle : MonoBehaviour
    {
        private IdleEvent idleEvent;
        private Rigidbody2D rb;

        private void Awake()
        {
            idleEvent = GetComponent<IdleEvent>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            idleEvent.OnIdle += IdleEvent_OnIdle;
        }

        private void OnDisable()
        {
            idleEvent.OnIdle += IdleEvent_OnIdle;
        }

        private void IdleEvent_OnIdle(IdleEvent idleEvent)
        {
            MoveRigidBody();
        }

        private void MoveRigidBody()
        {
            rb.velocity = Vector2.zero;
        }
    }
}