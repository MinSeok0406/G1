using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 장난감 정리 미니게임
    /// 흩어진 장난감을 드래그하여 상자에 정리하는 게임
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class ToySortMiniGame : MiniGameBase
    {
        [SerializeField] private Transform toysParent;
        [SerializeField] private Transform toyBoxTransform; // 장난감 상자 위치

        private List<ToyObject> toys = new();
        private int sortedCount = 0;
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

            sortedCount = 0;
            toys.Clear();

            foreach (Transform child in toysParent)
            {
                if (child.TryGetComponent(out ToyObject toy))
                {
                    toy.OnSorted += OnToySorted;
                    toys.Add(toy);
                }
            }
        }

        private void OnToySorted()
        {
            sortedCount++;

            if (sortedCount >= toys.Count)
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
            // 장난감들을 랜덤 위치에 배치
            foreach (ToyObject toy in toys)
            {
                toy.gameObject.SetActive(true);
                toy.ResetToRandomPosition();
            }
        }

        /// <summary>
        /// 미션 초기화 (라운드 시작 시)
        /// </summary>
        public void ResetMission()
        {
            sortedCount = 0;

            foreach (ToyObject toy in toys)
            {
                toy.ResetToRandomPosition();
                toy.gameObject.SetActive(true);
            }
        }
    }
}
