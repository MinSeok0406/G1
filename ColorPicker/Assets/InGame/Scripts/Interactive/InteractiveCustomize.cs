using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 커스터마이즈 상호작용 오브젝트
    /// </summary>
    public class InteractiveCustomize : InteractiveTriggerBase
    {
        [Header("Customize UI")]
        [SerializeField] private GameObject customizeUIPanel;

        protected override bool CanInteract()
        {
            // 항상 상호작용 가능
            return true;
        }

        protected override void HandleInteractLocal()
        {
            // UI 표시
            ShowCustomizeUI();

            // 버튼/하이라이트 정리
            CleanupLocalInteraction();
        }

        private void ShowCustomizeUI()
        {
            if (customizeUIPanel != null)
            {
                customizeUIPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[InteractiveCustomize] Customize UI panel is not assigned!");
            }
        }
    }
}
