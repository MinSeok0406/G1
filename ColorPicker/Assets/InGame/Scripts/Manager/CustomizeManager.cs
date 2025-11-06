using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 플레이어 커스터마이즈 색상 관리 (어몽어스 스타일)
    /// </summary>
    public sealed class CustomizeManager : SingletonNetworkBehaviour<CustomizeManager>
    {
        [Header("Available Customize Colors")]
        [SerializeField] private Color[] availableColors = new Color[]
        {
            new Color(1f, 0f, 0f),      // Red
            new Color(0f, 0f, 1f),      // Blue
            new Color(0f, 1f, 0f),      // Green
            new Color(1f, 0.75f, 0.8f), // Pink
            new Color(1f, 0.5f, 0f),    // Orange
            new Color(1f, 1f, 0f),      // Yellow
            Color.black,                 // Black
            Color.white,                 // White
            new Color(0.5f, 0f, 0.5f),  // Purple
            new Color(0.65f, 0.16f, 0.16f), // Brown
            new Color(0f, 1f, 1f),      // Cyan
            new Color(0.5f, 1f, 0f),    // Lime
        };

        // 호스트가 관리하는 색상 할당 상태
        private readonly Dictionary<int, int> _playerColorAssignments = new Dictionary<int, int>(); // ActorNumber -> ColorIndex
        private readonly HashSet<int> _usedColorIndices = new HashSet<int>();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        #region Public API

        /// <summary>
        /// [Client] 색상 변경 요청
        /// </summary>
        public void RequestColorChange(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= availableColors.Length)
            {
                Debug.LogWarning($"[CustomizeManager] Invalid color index: {colorIndex}");
                return;
            }

            int myActorNum = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
            if (myActorNum < 0) return;

            photonView.RPC(nameof(RPC_RequestColorChange), RpcTarget.MasterClient, myActorNum, colorIndex);
        }

        /// <summary>
        /// 색상 가져오기
        /// </summary>
        public Color GetColor(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= availableColors.Length)
                return Color.gray;

            return availableColors[colorIndex];
        }

        /// <summary>
        /// 플레이어의 현재 색상 인덱스 가져오기
        /// GameDataManager를 우선적으로 확인하여 데이터 일관성 보장
        /// </summary>
        public int GetPlayerColorIndex(int actorNumber)
        {
            // 1순위: GameDataManager에서 로드 (영구 저장소)
            int colorIndex = LoadColorFromGameData(actorNumber);
            if (colorIndex >= 0)
            {
                // GameDataManager에 데이터가 있으면 메모리 상태도 동기화
                if (!_playerColorAssignments.ContainsKey(actorNumber))
                {
                    _playerColorAssignments[actorNumber] = colorIndex;
                    _usedColorIndices.Add(colorIndex);
                }
                return colorIndex;
            }

            // 2순위: 메모리에서 확인 (GameDataManager에 아직 저장되지 않은 경우)
            if (_playerColorAssignments.TryGetValue(actorNumber, out colorIndex))
                return colorIndex;

            return -1;
        }

        /// <summary>
        /// 사용 가능한 색상 목록
        /// </summary>
        public List<int> GetAvailableColorIndices()
        {
            var available = new List<int>();
            for (int i = 0; i < availableColors.Length; i++)
            {
                if (!_usedColorIndices.Contains(i))
                {
                    available.Add(i);
                }
            }
            return available;
        }

        /// <summary>
        /// 전체 색상 배열 (UI용)
        /// </summary>
        public Color[] GetAllColors()
        {
            return availableColors;
        }

        #endregion

        #region RPC Methods

        [PunRPC]
        private void RPC_RequestColorChange(int actorNumber, int requestedColorIndex, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 요청자 검증
            if (info.Sender.ActorNumber != actorNumber)
            {
                Debug.LogWarning($"[CustomizeManager] Actor number mismatch. Rejected.");
                return;
            }

            // 색상 인덱스 유효성 검증
            if (requestedColorIndex < 0 || requestedColorIndex >= availableColors.Length)
            {
                photonView.RPC(nameof(RPC_ColorChangeRejected), info.Sender, "Invalid color index");
                return;
            }

            // 이미 사용 중인 색상인지 확인
            if (_usedColorIndices.Contains(requestedColorIndex))
            {
                // 자기 자신이 사용 중인 색상인지 확인
                if (_playerColorAssignments.TryGetValue(actorNumber, out int currentColor) && currentColor == requestedColorIndex)
                {
                    // 이미 같은 색상 사용 중
                    photonView.RPC(nameof(RPC_ColorChangeConfirmed), RpcTarget.All, actorNumber, requestedColorIndex);
                    return;
                }

                photonView.RPC(nameof(RPC_ColorChangeRejected), info.Sender, "Color already in use");
                return;
            }

            // 기존 색상 해제
            if (_playerColorAssignments.TryGetValue(actorNumber, out int oldColorIndex))
            {
                _usedColorIndices.Remove(oldColorIndex);
            }

            // 새 색상 할당
            _playerColorAssignments[actorNumber] = requestedColorIndex;
            _usedColorIndices.Add(requestedColorIndex);

            // 모든 클라이언트에 색상 변경 알림
            photonView.RPC(nameof(RPC_ColorChangeConfirmed), RpcTarget.All, actorNumber, requestedColorIndex);

            Debug.Log($"[CustomizeManager] Player {actorNumber} color changed to {requestedColorIndex}");
        }

        [PunRPC]
        private void RPC_ColorChangeConfirmed(int actorNumber, int colorIndex)
        {
            // 로컬 데이터 동기화
            if (_playerColorAssignments.ContainsKey(actorNumber))
            {
                int oldIndex = _playerColorAssignments[actorNumber];
                _usedColorIndices.Remove(oldIndex);
            }

            _playerColorAssignments[actorNumber] = colorIndex;
            _usedColorIndices.Add(colorIndex);

            // 플레이어 색상 적용
            ApplyColorToPlayer(actorNumber, colorIndex);

            // GameDataManager에 저장
            SaveColorToGameData(actorNumber, colorIndex);
        }

        [PunRPC]
        private void RPC_ColorChangeRejected(string reason)
        {
            UIManager.Instance?.ShowToastToScreen($"색상 변경 실패: {reason}");
        }

        /// <summary>
        /// [Host Only] 플레이어에게 사용 가능한 색상 자동 할당
        /// </summary>
        public void AssignRandomAvailableColor(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 이미 색상이 할당되어 있으면 스킵
            if (_playerColorAssignments.ContainsKey(actorNumber))
                return;

            var availableIndices = GetAvailableColorIndices();
            if (availableIndices.Count == 0)
            {
                Debug.LogWarning($"[CustomizeManager] No available colors for actor {actorNumber}");
                return;
            }

            // 랜덤 색상 선택
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

            _playerColorAssignments[actorNumber] = randomIndex;
            _usedColorIndices.Add(randomIndex);

            // 모든 클라이언트에 알림
            photonView.RPC(nameof(RPC_ColorChangeConfirmed), RpcTarget.All, actorNumber, randomIndex);

            Debug.Log($"[CustomizeManager] Auto-assigned color {randomIndex} to actor {actorNumber}");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 플레이어에게 색상 적용
        /// </summary>
        private void ApplyColorToPlayer(int actorNumber, int colorIndex)
        {
            // 로비 플레이어 찾기
            var lobbyPlayers = FindObjectsOfType<LobbyPlayer>();
            foreach (var lobbyPlayer in lobbyPlayers)
            {
                if (lobbyPlayer.photonView != null && lobbyPlayer.photonView.Owner.ActorNumber == actorNumber)
                {
                    lobbyPlayer.ApplyCustomizeColor(GetColor(colorIndex));
                    return;
                }
            }

            // 인게임 플레이어 찾기
            var players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.photonView != null && player.photonView.Owner.ActorNumber == actorNumber)
                {
                    player.ApplyCustomizeColor(GetColor(colorIndex));
                    return;
                }
            }
        }

        /// <summary>
        /// GameDataManager에 색상 저장
        /// </summary>
        private void SaveColorToGameData(int actorNumber, int colorIndex)
        {
            if (GameDataManager.Instance == null) return;

            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNumber, out var playerData))
            {
                // customizationData가 null이면 초기화
                if (playerData.customizationData == null)
                {
                    playerData.customizationData = new PlayerCustomizationData();
                }

                // 색상 인덱스 저장
                playerData.customizationData.customColorId = colorIndex;

                // 업데이트된 데이터를 GameDataManager에 반영 (호스트만)
                if (PhotonNetwork.IsMasterClient)
                {
                    GameDataManager.Instance.UpdatePublicPlayerData(playerData);
                    Debug.Log($"[CustomizeManager] Saved color {colorIndex} for player {playerData.nickname}");
                }
            }
        }

        /// <summary>
        /// GameDataManager에서 색상 로드 (Primary Source)
        /// </summary>
        private int LoadColorFromGameData(int actorNumber)
        {
            if (GameDataManager.Instance == null)
            {
                Debug.LogWarning($"[CustomizeManager] GameDataManager not available for actor {actorNumber}");
                return -1;
            }

            if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNumber, out var playerData))
            {
                if (playerData.customizationData != null)
                {
                    int colorIndex = playerData.customizationData.customColorId;
                    if (colorIndex >= 0 && colorIndex < availableColors.Length)
                    {
                        Debug.Log($"[CustomizeManager] Loaded color {colorIndex} from GameData for actor {actorNumber} ({playerData.nickname})");
                        return colorIndex;
                    }
                    else
                    {
                        Debug.LogWarning($"[CustomizeManager] Invalid color index {colorIndex} in GameData for actor {actorNumber}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[CustomizeManager] No customization data found in GameData for actor {actorNumber}");
                }
            }
            else
            {
                Debug.LogWarning($"[CustomizeManager] No player data found in GameDataManager for actor {actorNumber}");
            }

            return -1;
        }

        /// <summary>
        /// GameDataManager로부터 모든 플레이어의 색상 정보를 동기화
        /// 스냅샷 수신 후 호출하여 메모리 캐시를 업데이트
        /// </summary>
        public void SyncFromGameDataManager()
        {
            if (GameDataManager.Instance == null)
            {
                Debug.LogWarning("[CustomizeManager] GameDataManager not available for sync");
                return;
            }

            var allPlayerData = GameDataManager.Instance.GetAllPublicPlayerData();
            if (allPlayerData == null || allPlayerData.Count == 0)
            {
                Debug.Log("[CustomizeManager] No player data to sync");
                return;
            }

            int syncedCount = 0;
            foreach (var playerData in allPlayerData)
            {
                if (playerData == null || playerData.customizationData == null) continue;

                int actorId = playerData.currentActorId;
                int colorIndex = playerData.customizationData.customColorId;

                if (colorIndex >= 0 && colorIndex < availableColors.Length)
                {
                    // 이미 할당된 플레이어인 경우 업데이트
                    if (_playerColorAssignments.TryGetValue(actorId, out int existingColorIndex))
                    {
                        // 색상이 변경된 경우
                        if (existingColorIndex != colorIndex)
                        {
                            _usedColorIndices.Remove(existingColorIndex);
                            _playerColorAssignments[actorId] = colorIndex;
                            _usedColorIndices.Add(colorIndex);
                            Debug.Log($"[CustomizeManager] Updated color {existingColorIndex} -> {colorIndex} for actor {actorId}");
                        }
                    }
                    else
                    {
                        // 새로운 플레이어 색상 할당
                        _playerColorAssignments[actorId] = colorIndex;
                        _usedColorIndices.Add(colorIndex);
                        syncedCount++;
                        Debug.Log($"[CustomizeManager] Synced color {colorIndex} for actor {actorId} from GameDataManager");
                    }
                }
            }

            Debug.Log($"[CustomizeManager] Synced {syncedCount} player color(s) from GameDataManager");
        }

        #endregion

        #region Photon Callbacks

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            if (!PhotonNetwork.IsMasterClient) return;

            int actorNumber = otherPlayer.ActorNumber;

            // 색상 해제
            if (_playerColorAssignments.TryGetValue(actorNumber, out int colorIndex))
            {
                _usedColorIndices.Remove(colorIndex);
                _playerColorAssignments.Remove(actorNumber);
                Debug.Log($"[CustomizeManager] Released color {colorIndex} from actor {actorNumber}");
            }
        }

        #endregion
    }
}
