

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {
        #region ref playerUI
        [Space(10)]
        [Header("PlayerUI")]
        #endregion
        public GameObject interactiveUI;

        #region Ref Class Ability UI
        [Space(10)]
        [Header("Class Ability UI")]
        #endregion
        public GameObject mafiaUI;
        public GameObject killUI;

        #region Func UI
        public GameObject meetingButtonUI;
        #endregion

        private bool isCooldown;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitalizedMafiaUI()
        {
           //Dictionary<int, PlayerData> playerDictionary = NetworkManager.Instance.GetPlayerDictionary();

        }

        public void SetPlayerUI(bool isActive)
        {
            interactiveUI.SetActive(isActive);

        }

        public void SetMafiaUI(bool isActive)
        {
            mafiaUI.SetActive(isActive);
            killUI.SetActive(isActive);
        }

        public Button GetKillButton()
        {
            return killUI.GetComponent<Button>();
        }

        public void InitializedInteractiveItemUI()
        {
            interactiveUI.GetComponent<Button>().interactable = false;  

            meetingButtonUI.SetActive(false);
        }

        public void UesInteractiveButton()
        {
            //NetworkManager.Instance.MyPlayer.playerControl.UseInteractive();
        }

        public void UpdatePlayerCooldownUI(float remainingTime)
        {
            if (!killUI.activeSelf) return;

            TMP_Text cooldownText = killUI.GetComponentInChildren<TMP_Text>();

            if(remainingTime > 0)
            {
                cooldownText.text = ((int)remainingTime).ToString();
                isCooldown = true;
            }
            else
            {
                cooldownText.text = "";
                isCooldown = false;
            }
        }

        public bool CheckCooldown()
        {
            return isCooldown;
        }
    }
}
