using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 로비 씬에서 사용되는 플레이어
    /// 카메라 설정 및 매니저 등록만 처리
    /// </summary>
    public sealed class LobbyPlayer : PlayerBase
    {
        [SerializeField] private TMPro.TMP_Text nicknameText;

        private PlayerCameraSetup _cameraSetup;

        protected override void Awake()
        {
            base.Awake();
            _cameraSetup = new PlayerCameraSetup(transform);
        }

        protected override void PerformInitialization()
        {
            SetupCamera();
            RegisterToManager();
            SetupNickname();
            LoadCustomizeColor();
        }

        private void SetupCamera()
        {
            _cameraSetup?.Setup();
        }

        private void RegisterToManager()
        {
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("[LobbyPlayer] PlayerManager 인스턴스를 찾을 수 없습니다.", this);
                return;
            }

            PlayerManager.Instance.SetMyLobbyPlayer(this);
        }

        private void SetupNickname()
        {
            if (nicknameText == null)
            {
                Debug.LogWarning("[LobbyPlayer] nicknameText is not assigned.", this);
                return;
            }

            if (photonView == null || photonView.Owner == null)
            {
                Debug.LogWarning("[LobbyPlayer] photonView or Owner is null.", this);
                return;
            }

            // 플레이어 닉네임 설정
            int actorNum = photonView.Owner.ActorNumber;
            string displayName = photonView.Owner.NickName; // Photon 기본 닉네임

            // GameDataManager에서 닉네임 가져오기 시도
            if (GameDataManager.Instance != null)
            {
                if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNum, out var publicData))
                {
                    if (!string.IsNullOrEmpty(publicData.nickname))
                    {
                        displayName = publicData.nickname;
                    }
                    else
                    {
                        Debug.LogWarning($"[LobbyPlayer] PlayerData exists but nickname is empty for actor {actorNum}. Using Photon nickname: {displayName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[LobbyPlayer] No PlayerData found in GameDataManager for actor {actorNum}. Using Photon nickname: {displayName}");
                }
            }
            else
            {
                Debug.LogWarning($"[LobbyPlayer] GameDataManager not available. Using Photon nickname: {displayName}");
            }

            nicknameText.text = displayName;
            nicknameText.color = Color.white; // 로비에서는 모든 플레이어 닉네임 흰색

            Debug.Log($"[LobbyPlayer] Nickname set to: {displayName} for actor {actorNum}");
        }

        private void LoadCustomizeColor()
        {
            if (CustomizeManager.Instance == null)
            {
                Debug.LogError("[LobbyPlayer] CustomizeManager instance not found. Cannot load color.", this);
                return;
            }

            int actorNumber = OwnerActorNumber;
            if (actorNumber < 0)
            {
                Debug.LogError($"[LobbyPlayer] Invalid actor number: {actorNumber}", this);
                return;
            }

            Debug.Log($"[LobbyPlayer] Loading customize color for actor {actorNumber}...");

            // GameDataManager에서 모든 플레이어 색상 동기화 (신규 입장자용)
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                CustomizeManager.Instance.SyncFromGameDataManager();
                Debug.Log($"[LobbyPlayer] Synced all player colors from GameDataManager");
            }

            // 호스트에게 색상 자동 할당 요청 (GameDataManager에 색상이 없으면)
            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                // 기존 플레이어들의 색상 먼저 동기화
                CustomizeManager.Instance.SyncFromGameDataManager();

                int existingColorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNumber);
                if (existingColorIndex < 0)
                {
                    Debug.Log($"[LobbyPlayer] No existing color found. Auto-assigning color for actor {actorNumber}");
                    CustomizeManager.Instance.AssignRandomAvailableColor(actorNumber);
                }
                else
                {
                    Debug.Log($"[LobbyPlayer] Found existing color {existingColorIndex} for actor {actorNumber}");
                    // 기존 색상을 다시 할당하여 RPC_ColorChangeConfirmed 호출
                    Color color = CustomizeManager.Instance.GetColor(existingColorIndex);
                    ApplyCustomizeColor(color);
                }
            }

            // 모든 존재하는 LobbyPlayer에게 색상 적용 (새 플레이어가 기존 플레이어 색상 보기)
            ApplyAllPlayerColors();

            // 내 색상 로드 및 적용
            int colorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNumber);
            if (colorIndex >= 0)
            {
                Color color = CustomizeManager.Instance.GetColor(colorIndex);
                ApplyCustomizeColor(color);
                Debug.Log($"[LobbyPlayer] Successfully applied color index {colorIndex} (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2}) for actor {actorNumber}");
            }
            else
            {
                Debug.LogWarning($"[LobbyPlayer] No color assigned for actor {actorNumber} after initialization", this);
            }
        }

        /// <summary>
        /// 씬에 있는 모든 LobbyPlayer에게 색상 적용
        /// </summary>
        private void ApplyAllPlayerColors()
        {
            var allLobbyPlayers = FindObjectsOfType<LobbyPlayer>();
            foreach (var lobbyPlayer in allLobbyPlayers)
            {
                if (lobbyPlayer == null || lobbyPlayer.photonView == null) continue;

                int playerActorNum = lobbyPlayer.photonView.Owner.ActorNumber;
                int playerColorIndex = CustomizeManager.Instance.GetPlayerColorIndex(playerActorNum);

                if (playerColorIndex >= 0)
                {
                    Color playerColor = CustomizeManager.Instance.GetColor(playerColorIndex);
                    lobbyPlayer.ApplyCustomizeColor(playerColor);
                    Debug.Log($"[LobbyPlayer] Applied color {playerColorIndex} to existing player {playerActorNum}");
                }
            }
        }
    }
}