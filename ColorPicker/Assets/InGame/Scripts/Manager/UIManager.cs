using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

namespace ColorPicker.InGame
{
    public class UIManager : SingletonNetworkBehaviour<UIManager>
    {
        [SerializeField] private Button interactButton;

        [Header("Info UI")]
        [SerializeField] private TMP_Text coinInfoText;
        [SerializeField] private TMP_Text PaintInfoText;
        [SerializeField] private Image colorIcon;
        [SerializeField] private Slider missionStatusBar;

        [Header("UI References")]
        [SerializeField] private TMP_Text messageText;    // TextMeshPro 텍스트

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.5f; // 페이드인/아웃 시간
        [SerializeField] private float showDuration = 2.0f; // 표시 유지 시간

        private UnityAction defaultClick; // 기본 콜백 캐싱
         private Coroutine currentToastRoutine;

        private MafiaAbility mafiaAbility => AbilityManager.Instance?.GetComponentInChildren<MafiaAbility>();

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

            UpdateMissionStatusBarUI(0f);
            UpdateCoinInfo(0);
            UpdatePaintInfo(0);
            
            messageText.alpha = 0f;
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
            missionStatusBar.value = percent;
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

        public void UpdateAbilityCooldownUI(int viewID, float remain)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView == null || targetView.Owner == null)
            {
                Debug.LogWarning($"[Ability] PhotonView {viewID} not found or has no owner.");
                return;
            }

            int actorNum = targetView.OwnerActorNr;
            var targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNum);

            photonView.RPC(nameof(RPC_UpdateAbilityCooldownUI), targetPlayer, remain);
        }

        [PunRPC]
        public void RPC_UpdateAbilityCooldownUI(float remain)
        {
            if (mafiaAbility)
                mafiaAbility.StartCooldownUI(remain);
        }

        /// <summary>
        /// 토스트 메시지 표시 (텍스트 알파로 페이드)
        /// </summary>
        public void ShowToast(string msg, Vector3 targetPos)
        {
            if (currentToastRoutine != null)
                StopCoroutine(currentToastRoutine);

            currentToastRoutine = StartCoroutine(Co_ShowToast(msg, targetPos));
        }

        private IEnumerator Co_ShowToast(string msg, Vector3 targetPos)
        {
            messageText.text = msg;
            messageText.transform.position = targetPos;

            yield return Fade(0f, 1f, fadeDuration);

            yield return new WaitForSeconds(showDuration);

            yield return Fade(1f, 0f, fadeDuration);

            currentToastRoutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, t / duration);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float a)
        {
            Color c = messageText.color;
            c.a = a;
            messageText.color = c;
        }
    }
    
}
