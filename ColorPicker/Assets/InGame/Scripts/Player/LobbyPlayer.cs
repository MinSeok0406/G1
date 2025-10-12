using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 로비 씬에서 사용되는 플레이어
    /// 카메라 설정 및 매니저 등록만 처리
    /// </summary>
    public sealed class LobbyPlayer : PlayerBase
    {
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
    }
}