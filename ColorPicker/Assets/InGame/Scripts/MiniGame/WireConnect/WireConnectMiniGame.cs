using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 전선 연결 미니게임
    /// 왼쪽의 전선을 드래그하여 오른쪽의 맞는 색상 단자에 연결
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class WireConnectMiniGame : MiniGameBase
    {
        [SerializeField] private List<WireObject> wires = new();

        private int connectedCount = 0;
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

            connectedCount = 0;

            foreach (var wire in wires)
            {
                wire.OnWireConnected += OnWireConnected;
            }
        }

        private void OnWireConnected()
        {
            connectedCount++;

            if (connectedCount >= wires.Count)
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
            foreach (var wire in wires)
            {
                wire.gameObject.SetActive(true);
                wire.ResetWire();
            }
        }

        public void ResetMission()
        {
            connectedCount = 0;

            foreach (var wire in wires)
            {
                wire.ResetWire();
            }
        }

        private void OnDestroy()
        {
            foreach (var wire in wires)
            {
                wire.OnWireConnected -= OnWireConnected;
            }
        }
    }
}
