using Photon.Pun;
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
        [SerializeField] private GameObject miniGameSet;
        [SerializeField] private GameObject miniGameScreenUI;
        [SerializeField] private RectTransform rowImage_RT;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private Camera CRTCamera;

        private GameObject currentUI;
        private Dictionary<MiniGameType, MiniGameTag> miniGameDictionary = new Dictionary<MiniGameType, MiniGameTag>();

        protected override void Awake()
        {
            base.Awake();

            MiniGameInputHandler.rawImageRect = rowImage_RT;
            MiniGameInputHandler.targetRect = targetRect;
            MiniGameInputHandler.rtCamera = CRTCamera;
        }

        private void Start()
        {
            Initialized();
        }

        void Update()
        {
            if (currentUI == null || !currentUI.activeSelf) return;

            MiniGameInputHandler.ProcessInput();
        }

        private void Initialized()
        {
            miniGameDictionary.Clear();

            List<MiniGameTag> miniGames = miniGameSet.GetComponentsInChildren<MiniGameTag>().ToList();

            foreach(MiniGameTag tag in miniGames)
            {
                if(!miniGameDictionary.ContainsKey(tag.miniGameType))
                {
                    miniGameDictionary.Add(tag.miniGameType, tag);
                    tag.gameObject.SetActive(false);
                }
            }
            
            miniGameSet.gameObject.SetActive(false);
        }

        public void StartMiniGame(MiniGameType miniGameType)
        {
            currentUI = miniGameDictionary[miniGameType].gameObject;
            var game = currentUI.GetComponent<MiniGameBase>();
            game.SetCallback(OnComplete);
            game.StartGame();

            miniGameSet.SetActive(true);
            miniGameScreenUI.SetActive(true);

            MiniGameInputContext.EnableInput();

            game.gameObject.SetActive(true);
        }

        private void OnComplete(MiniGameReport report)
        {
            currentUI.SetActive(false);
            currentUI = null;

            miniGameSet.SetActive(false);
            miniGameScreenUI.SetActive(false);

            MiniGameInputContext.DisableInput();

            if (report.success)
            {
                MissionManager.Instance.RequestMissionComplete(report.playerId, (int)report.miniGameType);
            }
            else
            {
                Debug.Log($"[MiniGame] {report.miniGameType} 실패 - 보상 없음");
            }
        }
    }
}