using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColorPicker.InGame
{
    public class GameSettings : SingletonNetworkBehaviour<GameSettings>
    {
        [HideInInspector] public GameRuleData gameRuleData;

        protected override void Awake()
        {
                base.Awake();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (isServer)
            {
                gameRuleData = new GameRuleData();
                gameRuleData.SetRecommendGameRule();
            }
        }

    }

    [Serializable]
    public class GameRuleData
    {
        private bool isRecommendRule;
        private float moveSpeed;
        private int meetingsTime;
        private int voteTime;
        private int killCooldown;
        private int colorPickColldown;
        private int maxPlayer;
        private int mafiaAmount;

        public void SetRecommendGameRule()
        {
            moveSpeed = 7;
            meetingsTime = 120;
            voteTime = 120;
            killCooldown = 90;
            colorPickColldown = 90;
            maxPlayer = 10;
            mafiaAmount = 1;
        }
    }

}
