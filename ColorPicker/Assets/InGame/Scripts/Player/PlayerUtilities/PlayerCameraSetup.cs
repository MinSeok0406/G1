using UnityEngine;

namespace ColorPicker.InGame
{
    public sealed class PlayerCameraSetup
    {
        private const float CAMERA_Z_OFFSET = -10f;

        private readonly Transform _playerTransform;
        private GameObject _cameraObject;

        public PlayerCameraSetup(Transform playerTransform)
        {
            _playerTransform = playerTransform ?? throw new System.ArgumentNullException(nameof(playerTransform));
        }

        /// <summary>
        /// 카메라 설정
        /// </summary>
        public void Setup()
        {
            _cameraObject = GetOrCreateCamera();

            if (_cameraObject == null)
            {
                Debug.LogError("[PlayerCameraSetup] 카메라 생성/찾기에 실패했습니다.");
                return;
            }

            AttachToPlayer();
        }

        private GameObject GetOrCreateCamera()
        {
            // 기존 카메라가 있으면 사용
            if (Camera.main != null)
            {
                return Camera.main.gameObject;
            }

            // 없으면 프리팹에서 생성
            if (GameResources.Instance?.mainCameraPrefab == null)
            {
                Debug.LogError("[PlayerCameraSetup] GameResources에 mainCameraPrefab이 설정되지 않았습니다.");
                return null;
            }

            return Object.Instantiate(GameResources.Instance.mainCameraPrefab);
        }

        private void AttachToPlayer()
        {
            if (_cameraObject == null) return;

            Transform cameraTransform = _cameraObject.transform;
            cameraTransform.SetParent(_playerTransform, false);
            cameraTransform.localPosition = new Vector3(0f, 0f, CAMERA_Z_OFFSET);
        }
    }
}