using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(MiniGameTag))]
    [DisallowMultipleComponent]
    public class MiniGameTrigger : MonoBehaviour, IInteractive
    {
        private InteractiveObjectEffect interactiveObjectEffect;
        private MiniGameTag miniGameTag;

        private void Awake()
        {
            interactiveObjectEffect = GetComponent<InteractiveObjectEffect>();
            miniGameTag = GetComponent<MiniGameTag>();
        }
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void OnInteract()
        {
            MiniGameManager.Instance.StartMiniGame(miniGameTag.miniGameType);
        }

        public void ToggleHighlight(bool active)
        {
            interactiveObjectEffect.SetOutline(active);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return; // 로컬 플레이어만 처리

            playerControl.interactionDetector.AddInteractable(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;

            if (!playerControl.photonView.IsMine) return;

            playerControl.interactionDetector.RemoveInteractable(this);
        }
    }
}
