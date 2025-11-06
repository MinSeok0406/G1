using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(PhotonView))]
    public class DetectiveAbility : TargetingAbilityBase
    {
        [Header("UI")]
        [SerializeField] private Button inspectButton;
        [SerializeField] private TMP_Text resultText;

        private bool usedThisRound;
        private int currentRoundIndex;

        #region Public API (호환 유지)
        public void InitAbility()
        {
            if (inspectButton)
            {
                inspectButton.onClick.RemoveAllListeners();
                inspectButton.onClick.AddListener(OnInspectPressed);
                inspectButton.gameObject.SetActive(false);
            }

            if (resultText) resultText.text = "";

            usedThisRound = false;
            currentRoundIndex = 0;
            ClearHighlight();
        }

        public void EnableAbility()
        {
            InitAbility();
            gameObject.SetActive(true);
            if (inspectButton) inspectButton.gameObject.SetActive(true);
            RefreshUI();
        }

        public void DisableAbility()
        {
            if (inspectButton)
            {
                inspectButton.onClick.RemoveListener(OnInspectPressed);
                inspectButton.gameObject.SetActive(false);
            }
            if (resultText) resultText.text = "";

            usedThisRound = false;
            ClearHighlight();
            gameObject.SetActive(false);
        }

        public void NotifyRound(int roundIndex)
        {
            if (roundIndex != currentRoundIndex)
            {
                currentRoundIndex = roundIndex;
                usedThisRound = false;
                if (resultText) resultText.text = "";
                RefreshUI();
            }
        }

        public void HandleInspectResult(int targetViewID, int colorId)
        {
            if (targetViewID < 0)
            {
                if (resultText) resultText.text = "No Target";
                return;
            }

            if (colorId < 0)
            {
                if (resultText) resultText.text = "Unknown";
                return;
            }

            usedThisRound = true;
            RefreshUI();
            ClearHighlight();

            if (resultText)
            {
                var colorName = ((ColorType)colorId).ToString();
                resultText.text = $"Nearest: {colorName}";
            }
        }
        #endregion

        #region Base overrides
        protected override bool ShouldPauseTargeting() => usedThisRound;

        protected override void OnTargetChanged(PhotonView newTarget)
        {
            // 타깃 유무에 따라 버튼 enable 갱신
            RefreshUI();
        }

        protected override void OnUiTick()
        {
            RefreshUI();
        }
        #endregion

        private void RefreshUI()
        {
            if (!inspectButton) return;
            inspectButton.interactable = !usedThisRound && currentTarget != null;
        }

        private void OnInspectPressed()
        {
            if (usedThisRound)
            {
                if (resultText) resultText.text = "Already used";
                return;
            }

            var target = currentTarget;
            if (!target)
            {
                if (resultText) resultText.text = "No Target";
                return;
            }

            var me = PlayerManager.Instance.GetMyPlayer();
            if (!me) return;

            int myViewID = me.photonView.ViewID;
            AbilityManager.Instance.RequestDetectiveInspectNearest(myViewID, target.ViewID, aimRadius);
        }
    }
}
