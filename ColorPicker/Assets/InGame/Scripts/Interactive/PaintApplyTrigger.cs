using System.Collections;
using System.Collections.Generic;
using ColorPicker.inGame;
using Photon.Pun;
using UnityEditor;
using UnityEngine;


namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    public class PaintApplyTrigger : MonoBehaviourPun, IInteractive
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private SpriteRenderer paintSignSprite;

        private OutlineMarker outlineMarker;
        private Collider2D triggerCollider;

        // 로컬 플레이어가 트리거 안에 있는 동안만 유지
        private PlayerControl localPlayerInRange;

        private bool isPaint = false;

        private void Awake()
        {
            outlineMarker = GetComponentInChildren<OutlineMarker>();
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null) triggerCollider.isTrigger = true;
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GameDataManager.Instance.RegistPaintObject(photonView.ViewID);
        }

        public Vector3 GetPosition() => transform.position;

        public void OnInteract()
        {
            if (isPaint) return; // 이미 도색 완료면 무시
            ColorObjectManager.Instance.RequestPaintApply(photonView.ViewID);
        }

        /// <summary>
        /// Host가 색상 적용을 확정해 클라들에 전파한 뒤 호출되는 비주얼 반영 메서드
        /// </summary>
        public void SetPaintColor(int colorType)
        {
            // -1이면 "미설정" 상태로 아이콘만 감추고 리턴
            if (colorType == -1)
            {
                paintSignSprite.gameObject.SetActive(false);
                isPaint = false; // 필요 시 재활성화를 고려한다면 false 유지
                return;
            }

            paintSignSprite.color = HelperUtilities.ToUnityColor((ColorType)colorType);
            paintSignSprite.gameObject.SetActive(true);

            isPaint = true;

            // ===== (중요) OnTriggerExit2D와 동일한 마무리 작업 =====
            CleanupLocalInteraction();   // 로컬 상호작용 제거 + 하이라이트 해제
            DisableTriggerAndSelf();     // 더 이상 트리거 이벤트 받지 않도록 비활성화
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
            if (isPaint) return;                         // 도색 완료 후엔 무시
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;
            if (!playerControl.photonView.IsMine) return;

            localPlayerInRange = playerControl;
            playerControl.interactionDetector.AddInteractable(this);
            ToggleHighlight(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControl>();
            if (playerControl == null) return;
            if (!playerControl.photonView.IsMine) return;

            // isPaint 여부와 무관하게 정리(도색 도중/직후에도 안전)
            if (ReferenceEquals(playerControl, localPlayerInRange))
                localPlayerInRange = null;

            playerControl.interactionDetector.RemoveInteractable(this);
            ToggleHighlight(false);
        }

        /// <summary>
        /// 로컬 플레이어 기준 상호작용/하이라이트 정리
        /// </summary>
        private void CleanupLocalInteraction()
        {
            if (localPlayerInRange != null)
            {
                localPlayerInRange.interactionDetector.RemoveInteractable(this);
                ToggleHighlight(false);
                localPlayerInRange = null;
            }
        }

        /// <summary>
        /// 이후 트리거 이벤트를 아예 받지 않도록 콜라이더/스크립트를 비활성화
        /// </summary>
        private void DisableTriggerAndSelf()
        {
            if (triggerCollider != null) triggerCollider.enabled = false;
            enabled = false; // 이 컴포넌트의 추가 콜백도 중단(원치 않으면 제거)
        }
    }
}

