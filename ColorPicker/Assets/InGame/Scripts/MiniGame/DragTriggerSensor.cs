using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class DragTriggerSensor : MonoBehaviour
    {
        public System.Action<Collider2D> OnTriggerEntered;
        public System.Action<Collider2D> OnTriggerStayed;
        public System.Action<Collider2D> OnTriggerExited;

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTriggerEntered?.Invoke(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            OnTriggerStayed?.Invoke(other);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            OnTriggerExited?.Invoke(other);
        }
    }
}
