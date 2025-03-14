using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class GameDataManager : SingletonNetworkBehaviour<GameDataManager>
    {
        private GameRuleSettings gameRules;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            S_InitializedGameRule();
        }

        private void S_InitializedGameRule()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            gameRules = new GameRuleSettings();

            gameRules.SetRuleSettingRecomend();
        }

        public GameRuleSettings S_GetGameRules()
        {
            return gameRules;
        }
    }
}
