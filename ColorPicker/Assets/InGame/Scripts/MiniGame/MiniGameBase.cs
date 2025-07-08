using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public abstract class MiniGameBase : MonoBehaviour
    {
        protected Action<MiniGameReport> onComplete;
        public void SetCallback(Action<MiniGameReport> callback) => onComplete = callback;

        public abstract void Initialize();
        public abstract void StartGame();
    }

    [System.Serializable]
    public class MiniGameReport
    {
        public int playerId;
        public MiniGameType miniGameType;
        public bool success;
    }
}