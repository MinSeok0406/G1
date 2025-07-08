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
        private Dictionary<MiniGameType, GameObject> minigameDictionary = new Dictionary<MiniGameType, GameObject>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MiniGameInputContext.EnableInput(); // 테스트 코드

            List<MiniGameTag> miniGames = GetComponentsInChildren<MiniGameTag>().ToList();

            foreach(MiniGameTag miniGame in miniGames)
            {
                minigameDictionary.Add(miniGame.miniGameType, miniGame.gameObject);
            }
        }

        void Update()
        {
            if (currentUI == null || !currentUI.activeSelf) return;

            MiniGameInputHandler.ProcessInput();
        }

        public void StartMiniGame(MiniGameType miniGameType)
        {
            currentUI = minigameDictionary[miniGameType];
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