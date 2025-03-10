using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame {
    public class GameManager : SingletonNetworkBehaviour<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }


    }
}
