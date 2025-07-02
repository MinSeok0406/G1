using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerControl : MonoBehaviourPun
    {
        private List<IInteractive> interactiveObjects = new List<IInteractive>();
        private IInteractive currentInteractiveObject;

        private Player player;
        private bool disableControl;
        private float moveSpeed;

        [HideInInspector] public CircleCollider2D circleCollider2D;

        private void Awake()
        {
            player = GetComponent<Player>();
            circleCollider2D = GetComponentInChildren<CircleCollider2D>();

            moveSpeed = Settings.moveSpeed;
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine || disableControl) return;

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

        public void DisablePlayerControl(bool disableControl)
        {
            this.disableControl = disableControl;

            if (disableControl)
            {
                player.idleEvent.CallIdleEvent();
            }
        }

        public void UseInteractive()
        {
            currentInteractiveObject.Interactive();
        }

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    if (!photonView.IsMine) return;


        //    collision.TryGetComponent<IInteractive>(out IInteractive interactiveItem);

        //    if (interactiveItem != null)
        //    {
        //        interactiveObjects.Add(interactiveItem);
        //    }


        //}

        //private void OnTriggerStay2D(Collider2D collision)
        //{
        //    if (!photonView.IsMine) return;


        //    currentInteractiveObject = FindClosestInteractive();

        //    if (currentInteractiveObject != null)
        //    {
        //        currentInteractiveObject.SetUI(true);
        //    }
        //}

        private IInteractive FindClosestInteractive()
        {
            if (interactiveObjects.Count == 0) return null;

            IInteractive closestItem = null;
            float closestDistanceSquared = float.MaxValue; 

            foreach (var interactiveItem in interactiveObjects)
            {
                float distanceSquared = (transform.position - interactiveItem.GetPosition()).sqrMagnitude;

                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestItem = interactiveItem; // 가장 가까운 오브젝트 업데이트
                }
            }

            return closestItem;
        }

        //private void OnTriggerExit2D(Collider2D collision)
        //{
        //    if (!photonView.IsMine) return;

        //    collision.TryGetComponent<IInteractive>(out IInteractive interactiveItem);

        //    if (interactiveItem != null)
        //    {
        //        interactiveObjects.Remove(interactiveItem);
        //    }

        //    if (interactiveObjects.Count == 0) currentInteractiveObject = null;

        //    if (currentInteractiveObject == null)
        //    {
        //        UIManager.Instance.InitializedInteractiveItemUI();
        //    }

        //}
    }
}