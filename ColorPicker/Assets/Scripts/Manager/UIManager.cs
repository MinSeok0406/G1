

using System.Collections.Generic;
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

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitalizedMafiaUI()
        {
           Dictionary<int, PlayerData> playerDictionary = NetworkManager.Instance.GetPlayerDictionary();


        }

        public void SetPlayerUI()
        {
            interactiveUI.SetActive(true);

            Debug.Log("a");
        }

        public void SetMafiaUI()
        {
            mafiaUI.SetActive(true);
            killUI.SetActive(true);
        }

        public Button GetKillButton()
        {
            return killUI.GetComponent<Button>();
        }
    }
}
