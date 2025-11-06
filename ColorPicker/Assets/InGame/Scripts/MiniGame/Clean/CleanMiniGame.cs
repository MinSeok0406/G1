using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(MiniGameTag))]
    public class CleanMiniGame : MiniGameBase
    {
        [SerializeField] private DraggableObject cleanerTool;
        [SerializeField] private Transform cleanablesParent;

        private List<CleanableObject> cleanables = new();
        private int cleanedCount = 0;
        private int playerId;

        private MiniGameTag miniGameTag;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            cleanedCount = 0;
            cleanables.Clear();

            foreach (Transform child in cleanablesParent)
            {
                if (child.TryGetComponent(out CleanableObject cleanable))
                {
                    cleanable.OnCleaned += OnCleanableObjectCleaned;
                    cleanables.Add(cleanable);
                }
            }
        }

        private void OnCleanableObjectCleaned()
        {
            cleanedCount++;

            if(cleanedCount >= cleanables.Count)
            {
                MiniGameReport repo = new MiniGameReport()
                {
                    playerId = playerId,
                    miniGameType = miniGameTag.miniGameType,
                    success = true
                };

                onComplete?.Invoke(repo);
            }
        }

        public override void StartGame()
        {
            foreach(CleanableObject cleanable in cleanables)
            {
                cleanable.gameObject.SetActive(true);
                cleanable.Initialized();
            }
        }

        /// <summary>
        /// 미션 초기화 (라운드 시작 시 호출)
        /// </summary>
        public void ResetMission()
        {
            cleanedCount = 0;

            foreach (CleanableObject cleanable in cleanables)
            {
                cleanable.gameObject.SetActive(true);
                cleanable.Initialized();
            }

            Debug.Log("[CleanMiniGame] Mission reset");
        }
    }
}
