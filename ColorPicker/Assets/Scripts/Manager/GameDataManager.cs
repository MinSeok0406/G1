using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameDataManager : SingletonNetworkBehaviour<GameDataManager>
    {
        [HideInInspector] public GameRuleSettings gameRules;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            gameRules = new GameRuleSettings();

            gameRules.SetRuleSettingRecomend();
        }

    }
}
