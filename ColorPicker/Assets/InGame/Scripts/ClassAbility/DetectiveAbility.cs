using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(PhotonView))]
    public class DetectiveAbility : MonoBehaviourPun
    {
        private static readonly Collider2D[] _buf = new Collider2D[10];

        [Header("UI")]
        [SerializeField] private Button inspectButton;
        [SerializeField] private TMP_Text resultText;

        [Header("Target (auto-aim by ray)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private LayerMask aimMask2D;
        [SerializeField] private float aimRayLength = 5f;

        private readonly Dictionary<int, (PhotonView view, float sqr)> seen = new Dictionary<int, (PhotonView view, float sqr)>();

        private Transform origin2D;
        private PhotonView pv;
        private bool usedThisRound = false;
        private int currentRoundIndex = 0;

        private float _uiTick;
        private float _aimTick;

        private PhotonView _outlinedView;
        private OutlineMarker _outlinedMarker;

        #region Init/Enable/Disable
        public void InitAbility()
        {
            pv = GetComponent<PhotonView>();

            if (inspectButton)
            {
                inspectButton.onClick.RemoveAllListeners();
                inspectButton.onClick.AddListener(OnInspectPressed);
                inspectButton.gameObject.SetActive(false);
            }

            if (resultText) resultText.text = "";


            usedThisRound = false;
            currentRoundIndex = 0;

            ClearOutline();
        }

        public void EnableAbility()
        {
            InitAbility();

            gameObject.SetActive(true);
            if (inspectButton) inspectButton.gameObject.SetActive(true);
            RefreshUI();
        }

        public void DisableAbility()
        {
            if (inspectButton)
            {
                inspectButton.onClick.RemoveListener(OnInspectPressed);
                inspectButton.gameObject.SetActive(false);
            }
            if (resultText) resultText.text = "";
            usedThisRound = false;

            ClearOutline();
            gameObject.SetActive(false);
        }
        #endregion

        public void NotifyRound(int roundIndex)
        {
            if (roundIndex != currentRoundIndex)
            {
                currentRoundIndex = roundIndex;
                usedThisRound = false;
                if (resultText) resultText.text = "";
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            if (inspectButton) inspectButton.interactable = !usedThisRound;
        }

        private void Update()
        {
            if (usedThisRound) return;

            _uiTick += Time.unscaledDeltaTime;
            if (_uiTick >= 1f)
            {
                _uiTick = 0f;
                RefreshUI();
            }

            _aimTick += Time.unscaledDeltaTime;
            if (_aimTick >= 0.1f)
            {
                _aimTick = 0f;
                TryAcquireTargetByRay();
            }
        }

        private void TryAcquireTargetByRay()
{
    origin2D ??= PlayerManager.Instance.GetMyPlayer()?.transform;
    if (!origin2D) return;

    int hitCount = Physics2D.OverlapCircleNonAlloc(origin2D.position, aimRayLength, _buf, aimMask2D);

    seen.Clear();

    PhotonView nearest = null;
    float bestSqr = float.MaxValue;

    for (int i = 0; i < hitCount; i++)
    {
        var col = _buf[i];
        if (!col) continue;

        var view = col.GetComponentInParent<PhotonView>();
        if (!view || view.ViewID <= 0 || view.IsMine) continue;

        float sqr = (view.transform.position - origin2D.position).sqrMagnitude;

        // 같은 뷰가 여러 콜라이더로 잡힐 수 있으니, 더 가까운 값만 유지
        if (seen.TryGetValue(view.ViewID, out var cur))
        {
            if (sqr < cur.sqr) seen[view.ViewID] = (view, sqr);
        }
        else
        {
            seen.Add(view.ViewID, (view, sqr));
        }
    }

    foreach (var kv in seen.Values)
    {
        if (kv.sqr < bestSqr)
        {
            bestSqr = kv.sqr;
            nearest = kv.view; // 이미 확보한 view 재사용 (PhotonView.Find 불필요)
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

    // 타깃이 있을 때만 버튼 활성
    inspectButton.interactable = (nearest != null);
}

        private void ApplyOutline(PhotonView view)
        {
            if (!outlineMaterial) return;
            _outlinedView = view;
            var marker = OutlineMarker.GetOrAdd(view.gameObject);
            _outlinedMarker = marker;
            marker?.Apply(outlineMaterial);
        }

        private void ClearOutline()
        {
            _outlinedMarker?.Clear();
            _outlinedMarker = null;
            _outlinedView = null;
        }

        private void OnInspectPressed()
        {
            if (usedThisRound) return;

            var target = _outlinedView;
            if (!target)
            {
                if (resultText) resultText.text = "No Target";
                return;
            }

            var myViewID = PlayerManager.Instance.GetMyPlayer().photonView.ViewID;

            AbilityManager.Instance.RequestDetectiveInspectNearest(myViewID, target.ViewID, aimRayLength);
        }

        public void HandleInspectResult(int targetViewID, int colorId)
        {
            if (targetViewID < 0)
            {
                if (resultText) resultText.text = "No Target";
                return;
            }

            if (colorId < 0)
            {
                if (resultText) resultText.text = "Unknown";
                return;
            }

            usedThisRound = true;
            RefreshUI();

            if (resultText)
            {
                var colorName = ((ColorType)colorId).ToString();
                resultText.text = $"Nearest: {colorName}";
            }
        }
    }
}
