using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MiniGameManager : SingletonMonobehaviour<MiniGameManager>
    {
        public Transform uiRoot;
        public RectTransform dragArea;

        public GameObject currentUI;
        private Dictionary<MiniGameType, MiniGameTag> miniGameDictionary = new Dictionary<MiniGameType, MiniGameTag>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MiniGameInputContext.EnableInput(); // 테스트 코드

            Initialized();
        }

        void Update()
        {
            if (currentUI == null || !currentUI.activeSelf) return;

            MiniGameInputHandler.ProcessInput();
        }

        private void Initialized()
        {
            List<MiniGameTag> miniGames = uiRoot.GetComponentsInChildren<MiniGameTag>().ToList();

            miniGames.Clear();

            foreach(MiniGameTag tag in miniGames)
            {
                if(!miniGameDictionary.ContainsKey(tag.miniGameType))
                {
                    miniGameDictionary.Add(tag.miniGameType, tag);
                }
            }
        }

        public void StartMiniGame(MiniGameType miniGameType)
        {
            currentUI = miniGameDictionary[miniGameType].gameObject;
            var game = currentUI.GetComponent<MiniGameBase>();
            game.SetCallback(OnComplete);
            game.StartGame();

            game.gameObject.SetActive(true);
        }

        private void OnComplete(MiniGameReport report)
        {
            currentUI.SetActive(false);
            currentUI = null;

            MiniGameInputContext.DisableInput();

            // 네트워크로 결과 보고 
        }
    }
}