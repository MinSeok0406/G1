using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {
        [SerializeField] private Button interactButton;

        [Header("Info UI")]
        [SerializeField] private TMP_Text coinInfoText;
        [SerializeField] private TMP_Text PaintInfoText;
        [SerializeField] private Image colorIcon;


        private UnityAction defaultClick; // 기본 콜백 캐싱

        protected override void Awake()
        {
            base.Awake();

            if (!interactButton)
            {
                Debug.LogError("[UIManager] interactButton is not assigned.");
                return;
            }


            defaultClick = OnClick_Interaction;
            WireDefaultClick();

            // 기본은 숨김 & 비활성
            interactButton.interactable = false;
        }

        /// <summary>
        /// show=true면 onClick이 있으면 그걸로, 없으면 기본콜백으로 연결.
        /// show=false면 버튼을 숨기고 기본콜백으로 되돌림(오래된 클로저 방지).
        /// </summary>
        public void ShowInteractionButton(bool show, UnityAction onClick = null)
        {
            if (!interactButton) return;

            if (show)
            {
                // 리스너 교체(중복 방지)
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(onClick ?? defaultClick);

                interactButton.interactable = true;
            }
            else
            {
                interactButton.interactable = false;

                WireDefaultClick();
            }
        }

        private void WireDefaultClick()
        {
            if (!interactButton) return;
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(defaultClick);
        }

        private void OnClick_Interaction()
        {
            var myPlayer = PlayerManager.Instance?.GetMyPlayer();
            if (!myPlayer)
            {
                Debug.LogWarning("[UIManager] Local player not found.");
                return;
            }

            var pc = myPlayer.GetComponent<PlayerControl>();
            if (!pc)
            {
                Debug.LogWarning("[UIManager] PlayerControl missing on local player.");
                return;
            }

            pc.TryInteract();
        }


        public void UpdateMissionStatusBarUI(float percent)
        {
            // TODO: 진행도 바 UI 반영 (필요 시)
        }

        public void UpdateCoinInfo(int coin)
        {
            if (coinInfoText)
                coinInfoText.text = $"x {coin}";
        }

        public void UpdatePaintInfo(int paint)
        {
            if (PaintInfoText)
                PaintInfoText.text = $"x {paint}";
        }
        

        public void BroadcastUpdateColorIcon()
        {
            photonView.RPC(nameof(UpdateColorIcon), RpcTarget.All);
        } 

        [PunRPC]
        public void UpdateColorIcon()
        {
            GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(PhotonNetwork.LocalPlayer.ActorNumber, out var data);

            if (colorIcon)
                colorIcon.color = HelperUtilities.ToUnityColor((ColorType)data.identityColorId);
        }
    }
}
