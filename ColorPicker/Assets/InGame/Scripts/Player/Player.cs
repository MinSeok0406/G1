using FunkyCode;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 인게임 씬에서 사용되는 플레이어
    /// 라이트, 사망 처리 등 게임 로직 포함
    /// </summary>
    public sealed class Player : PlayerBase
    {
        [SerializeField] private Light2D playerLight;

        private DeathEvent _deathEvent;
        private PlayerCameraSetup _cameraSetup;
        private PlayerLightController _lightController;

        public DeathEvent DeathEvent => _deathEvent ??= GetComponent<DeathEvent>();
        public int OwnerActNum { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            _cameraSetup = new PlayerCameraSetup(transform);
            _lightController = new PlayerLightController(playerLight);
        }

        protected override void PerformInitialization()
        {
            SetupCamera();
            EnableLight();
            CacheOwnerInfo();
            RegisterToManager();
        }

        private void SetupCamera()
        {
            _cameraSetup?.Setup();
        }

        private void EnableLight()
        {
            _lightController?.Enable();
        }

        private void CacheOwnerInfo()
        {
            OwnerActNum = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
        }

        private void RegisterToManager()
        {
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("[Player] PlayerManager 인스턴스를 찾을 수 없습니다.", this);
                return;
            }

            PlayerManager.Instance.SetMyPlayer(this);
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        public void Die()
        {
            DeathEvent?.CallDeathEvent();
            _lightController?.Disable();
        }

        /// <summary>
        /// 생존 여부 확인
        /// </summary>
        public bool IsAlive()
        {
            if (!GameDataManager.Instance.TryGetInGameDataByActorId(OwnerActNum, out var data))
            {
                return true;
            }

            return data.isAlive;
        }
    }
}