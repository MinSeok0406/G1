using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class PaintApplyTrigger : InteractiveTriggerBase
    {
        [SerializeField] private SpriteRenderer paintSignSprite;

        private bool isPainted;

        protected override void Awake()
        {
            base.Awake();

            // 초기 아이콘 상태
            if (paintSignSprite) paintSignSprite.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
                GameDataManager.Instance.RegistPaintObject(photonView.ViewID);
        }

        protected override bool CanInteract()
        {
            return !isPainted; // 이미 도색 완료면 상호작용 불가
        }

        protected override void HandleInteractLocal()
        {
            if (isPainted) return;

            // 호스트에게 도색 요청 (ColorObjectManager에서 승인/동기화)
            ColorObjectManager.Instance.RequestPaintApply(photonView.ViewID);
            // 승인 전에는 UI/하이라이트 유지 (사용자 피드백)
        }

        /// <summary>
        /// 호스트 승인 후, 모든 클라에서 비주얼 반영 (네트워크 동기화 지점)
        /// </summary>
        public void SetPaintColor(int colorType)
        {
            // -1: 미설정 상태로 되돌림
            if (colorType == -1)
            {
                if (paintSignSprite) paintSignSprite.gameObject.SetActive(false);
                isPainted = false;
                return;
            }

            if (paintSignSprite)
            {
                paintSignSprite.color = HelperUtilities.ToUnityColor((ColorType)colorType);
                paintSignSprite.gameObject.SetActive(true);
            }

            isPainted = true;

            // 완료 시 상호작용 정리(버튼/하이라이트/Detector)
            CleanupLocalInteraction();

            // 이후 트리거 콜백 중단(선택) – 원한다면 주석 처리
            if (triggerCollider) triggerCollider.enabled = false;
            enabled = false;
        }
    }
}
