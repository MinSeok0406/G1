using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PlayerControl : MonoBehaviourPun
    {
        private List<IInteractive> interactiveObjects = new List<IInteractive>();                       

        private Player player;
        private bool disableControl;
        private float moveSpeed;

        public InteractionDetector interactionDetector;

        public IInteractive currentInteractive; // InteractionDetector가 세팅

        [HideInInspector] public CircleCollider2D circleCollider2D;

        private void Awake()
        {
            player = GetComponent<Player>();
            circleCollider2D = GetComponentInChildren<CircleCollider2D>(includeInactive: true);
            interactionDetector = GetComponentInChildren<InteractionDetector>(includeInactive: true);

            moveSpeed = Settings.moveSpeed;

            // 로컬 소유자만 인터랙션 감지 사용
            if (interactionDetector)
                interactionDetector.enabled = photonView.IsMine;
        }

        private void OnEnable()
        {
            if (interactionDetector)
                interactionDetector.enabled = photonView.IsMine;
        }

        private void Update()
        {
            if (!photonView.IsMine || disableControl) return;

            // 추가 소유자 체크 필요 시 유지
            if (player.ownerActNum != PhotonNetwork.LocalPlayer.ActorNumber) return;

            MoveInput();
        }

        private void MoveInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector2 dir = new Vector2(h, v);
            if (h != 0f && v != 0f) dir = dir.normalized;

            if (dir != Vector2.zero)
                player.movementByVelocityEvent.CallMovementByVElocityEvent(dir, moveSpeed);
            else
                player.idleEvent.CallIdleEvent();
        }

        public void DisablePlayerControl(bool disableControl)
        {
            this.disableControl = disableControl;
            if (disableControl)
                player.idleEvent.CallIdleEvent();
        }

        public void TryInteract()
        {
            // 재검증 가드
            if (currentInteractive == null || (currentInteractive as Object) == null)
                return;

            currentInteractive.OnInteract();
        }

        // 선택: 외부에서 안전히 설정하도록 메서드 제공(필드 직접 접근도 유지)
        public void SetCurrentInteractive(IInteractive target)
        {
            currentInteractive = target;
        }

    }
}
