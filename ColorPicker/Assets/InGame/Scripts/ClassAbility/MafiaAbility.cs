using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(PhotonView))]
    public class MafiaAbility : TargetingAbilityBase
    {
        private static readonly WaitForSeconds k_1sec = new WaitForSeconds(1f);

        [Header("UI")]
        [SerializeField] private Button killButton;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private ColorPickKillButton colorPickKillButton;

        private Coroutine cooldownRoutine;

        #region Public API (호환 유지)
        public void InitAbility()
        {
            if (killButton)
            {
                killButton.onClick.RemoveAllListeners();
                killButton.onClick.AddListener(OnKillButtonPressed);
                killButton.gameObject.SetActive(false);
            }

            if (colorPickKillButton)
            {
                var btn = colorPickKillButton.GetColorPickKillButton();
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnColorPickKillButtonPressed);
                }
                colorPickKillButton.gameObject.SetActive(false);
            }

            if (cooldownText) cooldownText.text = "";

            AbilityManager.Instance.RequestMafiaLayerSync(PhotonNetwork.LocalPlayer.ActorNumber);
            ClearHighlight();
        }

        public void EnableAbility()
        {
            InitAbility();
            gameObject.SetActive(true);

            if (killButton) killButton.gameObject.SetActive(true);
            if (colorPickKillButton) colorPickKillButton.gameObject.SetActive(true);
        }

        public void DisableAbility()
        {
            if (killButton)
            {
                killButton.onClick.RemoveListener(OnKillButtonPressed);
                killButton.gameObject.SetActive(false);
            }

            if (colorPickKillButton)
            {
                var btn = colorPickKillButton.GetColorPickKillButton();
                if (btn) btn.onClick.RemoveAllListeners();
                colorPickKillButton.gameObject.SetActive(false);
            }

            ClearHighlight();
            gameObject.SetActive(false);
        }

        public void StartCooldownUI(float duration)
        {
            if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
            colorPickKillButton?.CoolInitialize();

            cooldownRoutine = StartCoroutine(Co_Cooldown(duration));
        }
        #endregion

        #region Base overrides
        protected override bool ShouldPauseTargeting() => cooldownRoutine != null;

        protected override void OnTargetChanged(PhotonView newTarget)
        {
            // 타깃이 있을 때만 킬 버튼 활성 후보
            UpdateKillButtonInteractable();
        }

        protected override void OnUiTick()
        {
            UpdateKillButtonInteractable();
        }
        #endregion

        #region UI/Action
        private void UpdateKillButtonInteractable()
        {
            if (!killButton) return;
            killButton.interactable = CanUseAbility();
        }

        private bool CanUseAbility()
        {
            return currentTarget && currentTarget.ViewID > 0 && cooldownRoutine == null;
        }

        private void OnKillButtonPressed()
        {
            if (!CanUseAbility()) return;

            int targetViewID = currentTarget.ViewID;
            var me = PlayerManager.Instance.GetMyPlayer();
            if (!me) return;
            int myViewID = me.photonView.ViewID;

            AbilityManager.Instance.TryRequestKill(targetViewID, myViewID, aimRadius);

            var killerPV = PhotonView.Find(myViewID);
            var targetPV = PhotonView.Find(targetViewID);

            killerPV.transform.position = targetPV.transform.position;
        }

        private void OnColorPickKillButtonPressed()
        {
            AbilityManager.Instance.TryRequestColorPick();
        }
        #endregion

        #region Cooldown UI
        private IEnumerator Co_Cooldown(float duration)
        {
            float end = Time.time + Mathf.Max(0f, duration);
            while (true)
            {
                float remain = end - Time.time;
                if (remain <= 0f)
                {
                    UpdateCooldownUI(0f);
                    colorPickKillButton?.CoolInitialize();
                    cooldownRoutine = null;
                    yield break;
                }

                UpdateCooldownUI(remain);
                yield return k_1sec; // 1초 단위 갱신
            }
        }

        private void UpdateCooldownUI(float remain)
        {
            if (cooldownText)
                cooldownText.text = remain > 0f ? Mathf.CeilToInt(remain).ToString() : "";

            if (remain > 0f) colorPickKillButton?.UpdateCooldownUI(remain);
            else colorPickKillButton?.CoolInitialize();
        }
        #endregion
    }
}
