using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractiveTriggerBase : MonoBehaviourPun, IInteractive
    {
        [Header("Highlight")]
        [SerializeField] private Material outlineMaterial;

        [Header("Toast/UI")]
        [SerializeField] protected Vector3 toastOffset;

        protected HighlightController highlight;
        protected Collider2D triggerCollider;
        protected PlayerControlBase localPlayerControl; // 트리거 안에 있는 로컬 플레이어 컨트롤
        private UnityAction _cachedClick;

        protected virtual void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider) triggerCollider.isTrigger = true;

            // 하이라이트 유틸 자동 부착/활용
            highlight = GetComponent<HighlightController>();
            if (!highlight) highlight = gameObject.AddComponent<HighlightController>();

            _cachedClick = OnInteractionClicked; // delegate 캐시 (GC 절감)
        }

        protected virtual void OnEnable()
        {
            // 기본적으로 서버/클라 모두 존재하지만 상호작용은 로컬만 수행
        }

        protected virtual void OnDisable()
        {
            // 안전 정리
            SetHighlight(false);
            HideButton();
            CleanupPlayerInteraction();
        }

        private void CleanupPlayerInteraction()
        {
            if (localPlayerControl != null)
            {
                // PlayerControl 또는 LobbyPlayerControl 모두 InteractionDetector 프로퍼티를 가짐
                var interactionDetector = GetInteractionDetector(localPlayerControl);
                if (interactionDetector != null)
                {
                    interactionDetector.RemoveInteractable(this);
                }
                localPlayerControl = null;
            }
        }

        private InteractionDetector GetInteractionDetector(PlayerControlBase playerControl)
        {
            if (playerControl is PlayerControl pc)
                return pc.InteractionDetector;
            if (playerControl is LobbyPlayerControl lpc)
                return lpc.InteractionDetector;
            return null;
        }

        // ===== IInteractive =====
        public virtual Vector3 GetPosition() => transform.position;

        public void ToggleHighlight(bool active) => SetHighlight(active);

        public void OnInteract() => HandleInteractLocal();

        // ===== 트리거 공통 처리 =====
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControlBase>();
            if (!playerControl || playerControl.photonView == null || !playerControl.photonView.IsMine) return;
            if (!CanInteract()) return; // 상황 제한(예: 이미 도색 완료)

            localPlayerControl = playerControl;

            var interactionDetector = GetInteractionDetector(playerControl);
            if (interactionDetector != null)
            {
                interactionDetector.AddInteractable(this);
            }

            SetHighlight(true);
            ShowButton();
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerControl = other.GetComponent<PlayerControlBase>();
            if (!playerControl || playerControl.photonView == null || !playerControl.photonView.IsMine) return;

            if (ReferenceEquals(playerControl, localPlayerControl))
                localPlayerControl = null;

            var interactionDetector = GetInteractionDetector(playerControl);
            if (interactionDetector != null)
            {
                interactionDetector.RemoveInteractable(this);
            }

            SetHighlight(false);
            HideButton();
        }

        protected void CleanupLocalInteraction()
        {
            CleanupPlayerInteraction();
            SetHighlight(false);
            HideButton();
        }

        // ===== 버튼/하이라이트 유틸 =====
        protected void ShowButton()
        {
            // 로비 또는 인게임에 따라 적절한 매니저 사용
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.ShowInteractionButton(true, _cachedClick);
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionButton(true, _cachedClick);
            }
        }

        protected void HideButton()
        {
            // 로비 또는 인게임에 따라 적절한 매니저 사용
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.ShowInteractionButton(false);
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionButton(false);
            }
        }

        protected void SetHighlight(bool on)
        {
            // IInteractive 호환용 ToggleHighlight는 이미 이 메서드를 호출
            // HighlightController 내부가 Sprite/Mesh 전부 처리
            if (highlight != null)
                highlight.SetHighlighted(on);
        }

        private void OnInteractionClicked()
        {
            // 버튼 클릭 -> 대상이 여전히 유효한지 확인 후 처리
            if (!CanInteract())
            {
                HideButton();
                return;
            }
            HandleInteractLocal();
        }

        // ===== 파생 클래스가 구현할 포인트 =====
        protected virtual bool CanInteract() => true;

        /// <summary>로컬 플레이어가 상호작용 버튼을 눌렀을 때의 동작</summary>
        protected abstract void HandleInteractLocal();
    }
}
