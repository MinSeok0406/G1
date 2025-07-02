using ColorPicker.InGame;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class LobbyPlayerControl : MonoBehaviourPun
    {
        private List<IInteractive> interactiveObjects = new List<IInteractive>();
        private IInteractive currentInteractiveObject;

        private LobbyPlayer player;
        private bool disableControl;
        private float moveSpeed;
        private bool isPlayerSlotsVisible = false;

        [HideInInspector] public CircleCollider2D circleCollider2D;

        private void Awake()
        {
            player = GetComponent<LobbyPlayer>();
            circleCollider2D = GetComponentInChildren<CircleCollider2D>();

            moveSpeed = Settings.moveSpeed;
        }

        private void Update()
        {
            if (!photonView.IsMine || disableControl) return;

            MoveInput();

            bool isTabDown = Input.GetKey(KeyCode.Tab);
            if (isTabDown != isPlayerSlotsVisible)
            {
                isPlayerSlotsVisible = isTabDown;
                LobbyUIManager.Instance.SetPlayerSlotsActive(isTabDown);
            }
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
