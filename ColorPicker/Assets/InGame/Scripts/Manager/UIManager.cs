using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {

        [SerializeField] private GameObject interactButton;
        private Button button;

        protected override void Awake()
        {
            base.Awake();
            button = interactButton.GetComponent<Button>();
            button.onClick.AddListener(OnClickInteraction);
           
        }

        public void ShowInteractionButton(bool show, UnityAction onClick = null)
        {
            button.interactable = show;
        }
        private void OnClickInteraction()
        {
            PlayerControl localPlayer;
            
            localPlayer = PlayerManager.Instance.GetMyPlayer().GetComponent<PlayerControl>();

            localPlayer?.TryInteract(); 
        }

        public void UpdateMissionStatusUI(PlayerMissionData mission)
        {
        }

        public void UpdateMissionStatusBarUI(float percent)
        {

        }
    }
}
