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
            string displayName = photonView.Owner.NickName; // 기본값

            // GameDataManager에서 닉네임 가져오기 시도
            if (GameDataManager.Instance != null &&
                GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNum, out var publicData))
            {
                if (!string.IsNullOrEmpty(publicData.nickname))
                {
                    displayName = publicData.nickname;
                }
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

            // 호스트에게 색상 자동 할당 요청 (GameDataManager에 색상이 없으면)
            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                int existingColorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorNumber);
                if (existingColorIndex < 0)
                {
                    Debug.Log($"[LobbyPlayer] No existing color found. Auto-assigning color for actor {actorNumber}");
                    CustomizeManager.Instance.AssignRandomAvailableColor(actorNumber);
                }
                else
                {
                    Debug.Log($"[LobbyPlayer] Found existing color {existingColorIndex} in GameDataManager for actor {actorNumber}");
                }
            }

            // GameDataManager 기반으로 색상 로드 및 적용
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

                // GameDataManager 상태 확인
                if (GameDataManager.Instance != null &&
                    GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actorNumber, out var playerData))
                {
                    Debug.LogWarning($"[LobbyPlayer] PlayerData exists but customization data is missing for {playerData.nickname}");
                }
            }
        }
    }
}