using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{

    public class InteractiveMeeting : MonoBehaviourPun, IInteractive
    {
        [SerializeField] private SpriteRenderer bodyRenderer; // 시체 색상 표시
        private string ownerUID;   // 사망한 플레이어 UID
        private int actorId;       // 사망한 플레이어 ActorId

        [SerializeField] private Material outlineMaterial;

        private OutlineMarker outlineMarker;
        private Collider2D triggerCollider;
        private PlayerControl localPlayerInRange;

        // ===== 초기화 (호스트 전용) =====
        public void Initialize(string uid, int actorId, ColorType color)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ownerUID = uid;
            this.actorId = actorId;
            SetBodyColor(color);
        }

        public void ToggleHighlight(bool active)
        {
            if (outlineMarker == null) return;

            if (active)
            {
                if (outlineMaterial != null)
                    outlineMarker.Apply(outlineMaterial);
            }
            else
            {
                outlineMarker.Clear();
            }
        }

        private void SetBodyColor(ColorType color)
        {
            if (!bodyRenderer) return;
            bodyRenderer.color = HelperUtilities.ToUnityColor(color);
        }

        // ===== IInteractive 구현 =====
        public Vector3 GetPosition() => transform.position;

        public void OnInteract()
        {
            GameManager.Instance.RequestPhaseChange(GameStateType.Meeting);
        }
        
         private void OnTriggerEnter2D(Collider2D other)
        {                     
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;
            if (!playerControl.photonView.IsMine) return;

            localPlayerInRange = playerControl;
            playerControl.InteractionDetector.AddInteractable(this);
            ToggleHighlight(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;
            if (!playerControl.photonView.IsMine) return;

            if (ReferenceEquals(playerControl, localPlayerInRange))
                localPlayerInRange = null;

            playerControl.InteractionDetector.RemoveInteractable(this);
            ToggleHighlight(false);
        }

        /// <summary>
        /// 로컬 플레이어 기준 상호작용/하이라이트 정리
        /// </summary>
        private void CleanupLocalInteraction()
        {
            if (localPlayerInRange != null)
            {
                localPlayerInRange.InteractionDetector.RemoveInteractable(this);
                ToggleHighlight(false);
                localPlayerInRange = null;
            }
        }
    }
}