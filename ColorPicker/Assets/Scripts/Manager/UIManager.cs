

using System.Collections.Generic;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public void InitalizedMafiaUI()
        {
           Dictionary<int, PlayerData> playerDictionary = NetworkManager.Instance.GetPlayerDictionary();


        }
    }
}
