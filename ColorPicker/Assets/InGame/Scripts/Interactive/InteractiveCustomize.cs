using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 커스터마이즈 상호작용 오브젝트
    /// 로비와 인게임 모두에서 사용 가능
    /// </summary>
    public class InteractiveCustomize : InteractiveTriggerBase
    {
        [Header("Customize UI")]
        [SerializeField] private GameObject customizeUIPanel;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer iconSprite; // 커스터마이즈 아이콘 (선택 사항)

        private bool isUIOpen = false;

        protected override void Awake()
        {
            base.Awake();

            // 초기 아이콘 상태
            if (iconSprite)
            {
                iconSprite.gameObject.SetActive(true);
            }

            // UI가 초기에 열려있다면 닫기
            if (customizeUIPanel != null)
            {
                customizeUIPanel.SetActive(false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // UI가 활성화되어 있다면 하이라이트 상태 반영
            if (isUIOpen && highlight != null)
            {
                highlight.SetHighlighted(true);
            }
        }

        protected override bool CanInteract()
        {
            // 항상 상호작용 가능 (UI 열기/닫기 모두 가능)
            return true;
        }

        protected override void HandleInteractLocal()
        {
            // UI 토글
            ToggleCustomizeUI();

            // 버튼만 숨기기 (하이라이트는 유지)
            // 사용자가 트리거 영역에 있는 동안 계속 상호작용 가능
            HideButton();
        }

        private void ToggleCustomizeUI()
        {
            if (customizeUIPanel == null)
            {
                Debug.LogWarning("[InteractiveCustomize] Customize UI panel is not assigned!");
                return;
            }

            isUIOpen = !isUIOpen;
            customizeUIPanel.SetActive(isUIOpen);

            if (isUIOpen)
            {
                Debug.Log("[InteractiveCustomize] Customize UI opened");
            }
            else
            {
                Debug.Log("[InteractiveCustomize] Customize UI closed");
            }
        }

        /// <summary>
        /// 외부에서 UI를 닫을 때 호출 (예: UI의 닫기 버튼)
        /// </summary>
        public void CloseUI()
        {
            if (customizeUIPanel != null)
            {
                customizeUIPanel.SetActive(false);
                isUIOpen = false;
                Debug.Log("[InteractiveCustomize] Customize UI closed externally");
            }
        }
    }
}
