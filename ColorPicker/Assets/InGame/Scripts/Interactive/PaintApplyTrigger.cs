using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PaintApplyTrigger : MonoBehaviourPun, IInteractive
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Vector3 offset;
        [SerializeField] private SpriteRenderer paintSignSprite;
        private OutlineMarker outlineMarker;

        private void Awake()
        {
            outlineMarker = GetComponentInChildren<OutlineMarker>();
        }
        
        public Vector3 GetPosition() => transform.position;

        public void OnInteract()
        {
            photonView.RPC(nameof(RPC_PaintApply), RpcTarget.MasterClient);
        }

        [PunRPC]
        public void RPC_PaintApply(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameDataManager.Instance.TryGetInGameDataByActorId(info.Sender.ActorNumber, out InGameData data);

            if (data.paintCount > 0)
            {
                data.paintCount--;
                photonView.RPC(nameof(RPC_ReciveApplyPaintSing), RpcTarget.All);
            }
            
        }

        [PunRPC]
        public void RPC_ReciveApplyPaintSing()
        {
            
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

         private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return;

            playerControl.interactionDetector.AddInteractable(this);
            ToggleHighlight(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return;

            playerControl.interactionDetector.RemoveInteractable(this);
            ToggleHighlight(false);
        }
    }
}
