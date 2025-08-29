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

        [Header("Targeting (2D circle)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float aimRadius = 5f;
        [SerializeField] private LayerMask aimMask2D;

        // Legacy 유지
        [Header("Target (legacy)")]
        [SerializeField] private PhotonView currentTargetView;
        private Player currentPlayer;

        private Transform origin2D;
        private KillEvent killEvent;
        private PhotonView pv;

        // Tick 캐시
        private float uiTick;
        private float aimTick;

        // Outline 관리
        private PhotonView _outlinedView;
        private OutlineMarker _outlinedMarker;

        #region Init / Enable / Disable
        public void InitAbility()
        {
            pv = GetComponent<PhotonView>();
            killEvent = GetComponent<KillEvent>() ?? gameObject.AddComponent<KillEvent>();

            if (killButton)
            {
                killButton.onClick.RemoveAllListeners();
                killButton.onClick.AddListener(OnKillButtonPressed);
                killButton.gameObject.SetActive(false);
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
        }

        public void DisableAbility()
        {
            if (killButton)
            {
                killButton.onClick.RemoveListener(OnKillButtonPressed);
                killButton.gameObject.SetActive(false);
            }
            ClearOutline();
            gameObject.SetActive(false);
        }
        #endregion

        private void OnEnable()
        {
            if (killEvent) killEvent.OnKill += KillEvent_OnKill;
        }

        private void OnDisable()
        {
            if (killEvent) killEvent.OnKill -= KillEvent_OnKill;
        }

        private void KillEvent_OnKill(KillEvent sender, KillEventArgs args)
        {
            if (!pv || !pv.IsMine) return;

            int targetViewID = args?.targetViewID ?? ResolveTargetViewID();
            if (targetViewID < 0) return;

            AbilityManager.Instance.TryRequestKill(targetViewID, pv.ViewID);
        }

        private void Update()
        {

            // UI 주기 갱신 (1초)
            uiTick += Time.unscaledDeltaTime;
            if (uiTick >= 1f)
            {
                uiTick = 0f;
                UpdateCooldownUI();
                UpdateButtonInteractable();
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

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin2D.position, aimRadius, _buf, aimMask2D);

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

            if (nearest && nearest != _outlinedView)
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
            _outlinedView = view;
            _outlinedMarker = OutlineMarker.GetOrAdd(view.gameObject);
            _outlinedMarker?.Apply(outlineMaterial);
        }

        private void ClearOutline()
        {
            _outlinedMarker?.Clear();
            _outlinedMarker = null;
            _outlinedView = null;
        }

        private void UpdateCooldownUI()
        {
            if (!cooldownText) return;
            float remain = AbilityManager.Instance.GetRemainingCooldown(pv.ViewID);
            cooldownText.text = remain > 0f ? Mathf.CeilToInt(remain).ToString() : "";
        }

        private void UpdateButtonInteractable()
        {
            if (!killButton) return;
            killButton.interactable = CanUseAbility();
        }

        private bool CanUseAbility()
        {
            return ResolveTargetViewID() >= 0 && !AbilityManager.Instance.IsOnCooldown(pv.ViewID);
        }

        private void OnKillButtonPressed()
        {
            if (!pv || !pv.IsMine) return;
            if (!CanUseAbility()) return;

            int targetViewID = ResolveTargetViewID();
            if (targetViewID < 0) return;

            AbilityManager.Instance.TryRequestKill(targetViewID, pv.ViewID);
        }

        // ===== Legacy Target API (호환 유지) =====
        public void SetCurrentTarget(PhotonView target)
        {
            if (target != _outlinedView)
            {
                ClearOutline();
                if (target) ApplyOutline(target);
            }
            currentTargetView = target;
            currentPlayer = target && target.Owner?.TagObject is Player p ? p : null;
        }

        public void SetCurrentPlayer(Player p)
        {
            currentPlayer = p;
            currentTargetView = null;
        }

        private int ResolveTargetViewID()
        {
            if (currentTargetView && currentTargetView.ViewID > 0)
                return currentTargetView.ViewID;

            if (_outlinedView && _outlinedView.ViewID > 0)
                return _outlinedView.ViewID;

            if (currentPlayer != null)
            {
                int actor = currentPlayer.ownerActNum;
                if (GameDataManager.Instance.TryGetPublicPlayerDataByActorId(actor, out var pdata))
                    if (GameDataManager.Instance.TryGetViewIDByUID(pdata.googleUID, out int vid) && vid > 0)
                        return vid;
            }
            return -1;
        }

        public Player GetCurrentPlayer() => currentPlayer;
    }
}
