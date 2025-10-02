using System;
using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(PhotonView))]
    public class MafiaAbility : MonoBehaviourPun
    {
        // 버퍼 크기 넉넉히 (GC 0, 동시에 여러 대상 허용)
        private static readonly Collider2D[] _buf = new Collider2D[16];

        [Header("UI")]
        [SerializeField] private Button killButton;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private ColorPickKillButton colorPickKillButton;

        [Header("Targeting (2D circle)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float killRadius = 1f;
        [SerializeField] private LayerMask aimMask2D;

        // Legacy 유지
        private Player currentPlayer;
        private Coroutine cooldownRoutine;
        private Transform origin2D;

        // Tick 캐시
        private float uiTick;
        private float aimTick;
    
        // Outline 관리
        private PhotonView outlinedView;
        private OutlineMarker outlinedMarker;
        

        #region Init / Enable / Disable
        public void InitAbility()
        {
            if (killButton)
            {
                killButton.onClick.RemoveAllListeners();
                killButton.onClick.AddListener(OnKillButtonPressed);
            }

            Button button = colorPickKillButton.GetColorPickKillButton();

            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnColorPickKillButtonPressed);
            }

            if (cooldownText) cooldownText.text = "";

            AbilityManager.Instance.RequestMafiaLayerSync(PhotonNetwork.LocalPlayer.ActorNumber);

            ClearOutline();
        }

        public void EnableAbility()
        {
            InitAbility();
            gameObject.SetActive(true);
            if (killButton) killButton.gameObject.SetActive(true);
            if (colorPickKillButton) colorPickKillButton.gameObject.SetActive(true);
        }

        public void DisableAbility()
        {
            if (killButton)
            {
                killButton.onClick.RemoveListener(OnKillButtonPressed);
                killButton.gameObject.SetActive(false);
            }

            Button button = colorPickKillButton.GetColorPickKillButton();

            if (button)
            {
                button.onClick.RemoveAllListeners();
            }

            if (colorPickKillButton) colorPickKillButton.gameObject.SetActive(false);

            ClearOutline();
            
            gameObject.SetActive(false);
        }
        
        #endregion

        private void Update()
        {
            // UI 주기 갱신 (0.1초)
            uiTick += Time.unscaledDeltaTime;
            if (uiTick >= 0.1f)
            {
                uiTick = 0f;
                UpdateButtonInteractable();
            }

            if (cooldownRoutine != null)
            {
                if (outlinedView) ClearOutline();

                return;
            }

            // 타깃 갱신 (0.1초)
            aimTick += Time.unscaledDeltaTime;
            if (aimTick >= 0.1f)
            {
                aimTick = 0f;
                TryAcquireTarget();
            }
        }

        private void TryAcquireTarget()
        {
            origin2D ??= PlayerManager.Instance.GetMyPlayer()?.transform;
            if (!origin2D) return;

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin2D.position, killRadius, _buf, aimMask2D);

            PhotonView nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var col = _buf[i];
                if (!col) continue;
                PhotonView view = col.GetComponentInParent<PhotonView>();

                if (!view || view.ViewID <= 0 || view.IsMine) continue;

                float sqr = (view.transform.position - origin2D.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = view;
                }
            }

            if (nearest && nearest != outlinedView)
            {
                ClearOutline();
                ApplyOutline(nearest);
            }
            else if (!nearest)
            {
                ClearOutline();
            }
        }

        private void ApplyOutline(PhotonView view)
        {
            if (!outlineMaterial || !view) return;
            outlinedView = view;
            outlinedMarker = OutlineMarker.GetOrAdd(view.gameObject);
            outlinedMarker?.Apply(outlineMaterial);
        }

        private void ClearOutline()
        {
            outlinedMarker?.Clear();
            outlinedMarker = null;
            outlinedView = null;
        }

        public void StartCooldownUI(float duration)
        {
            // 기존 코루틴이 돌고 있다면 정지
            if (cooldownRoutine != null)
                StopCoroutine(cooldownRoutine);

            colorPickKillButton.CoolInitialize();

            float endTime = Time.time + duration;
            cooldownRoutine = StartCoroutine(UpdateCooldownUICoroutine(endTime));
        }

        private void UpdateCooldownUI(float remain)
        {
            if (!cooldownText) return;
            cooldownText.text = remain > 0f ? Mathf.CeilToInt(remain).ToString() : "";

            if (remain > 0f)
            {
                colorPickKillButton.UpdateCooldownUI(remain);
            }
            else
            {
                colorPickKillButton.CoolInitialize();
            }
        }

        private IEnumerator UpdateCooldownUICoroutine(float endTime)
        {
            while (true)
            {
                float remain = endTime - Time.time;
                if (remain <= 0f)
                {
                    UpdateCooldownUI(0f);
                    colorPickKillButton.CoolInitialize();
                    cooldownRoutine = null;
                    yield break;
                }

                UpdateCooldownUI(remain);

                // 1초 단위로만 갱신
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateButtonInteractable()
        {
            if (!killButton) return;

            bool canUsed = CanUseAbility();

            killButton.interactable = canUsed;

        }

        private bool CanUseAbility()
        {
            return ResolveTargetViewID() >= 0 && cooldownRoutine == null;
        }

        private void OnKillButtonPressed()
        {
            if (!CanUseAbility()) return;

            int targetViewID = ResolveTargetViewID();
            if (targetViewID < 0) return;

            int myViewID = PlayerManager.Instance.GetMyPlayer().photonView.ViewID;
            AbilityManager.Instance.TryRequestKill(targetViewID, myViewID, killRadius);
        }

        private void OnColorPickKillButtonPressed()
        {
            TryRequestColorPick();
        }

        private void TryRequestColorPick()
        {
            AbilityManager.Instance.TryRequestColorPick();
        }

        private int ResolveTargetViewID()
        {
            if (outlinedView && outlinedView.ViewID > 0)
                return outlinedView.ViewID;

            return -1;
        }

        public Player GetCurrentPlayer() => currentPlayer;
    }
}
