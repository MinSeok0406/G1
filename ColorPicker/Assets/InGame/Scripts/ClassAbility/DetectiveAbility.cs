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
        [SerializeField] private TMP_Text usageText;

        [Header("Target (auto-aim by ray)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private LayerMask aimMask2D;
        [SerializeField] private float aimRayLength = 5f;

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
            if (usageText) usageText.text = "Ready";

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
            if (usageText) usageText.text = "Ready";
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
            if (usageText) usageText.text = usedThisRound ? "Used" : "Ready";
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

            // 원형 범위 안에서 가장 가까운 1명 선택 (Overlap은 순서 미보장)
            int hitCount = Physics2D.OverlapCircleNonAlloc(origin2D.position, aimRayLength, _buf, aimMask2D);

            PhotonView nearest = null;
            float bestSqr = float.MaxValue;

            // 같은 플레이어(동일 PhotonView)에 여러 콜라이더가 붙어있을 수 있으니
            // ViewID 기준으로 최소 거리만 유지
            // (가벼운 구현: 처음 본 View만 반영. 더 정확히 하려면 Dictionary로 min 갱신)
            var seen = new System.Collections.Generic.Dictionary<int, float>();

            for (int i = 0; i < hitCount; i++)
            {
                var col = _buf[i];
                if (!col) continue;

                var view = col.GetComponentInParent<PhotonView>();
                if (!view || view.ViewID <= 0 || view.IsMine) continue;

                float sqr = (view.transform.position - origin2D.position).sqrMagnitude;

                if (seen.TryGetValue(view.ViewID, out float prev))
                {
                    if (sqr < prev) seen[view.ViewID] = sqr;
                }
                else
                {
                    seen.Add(view.ViewID, sqr);
                }
            }

            foreach (var kv in seen)
            {
                // kv.Key = ViewID, kv.Value = min sqr distance
                var candidate = PhotonView.Find(kv.Key);
                if (!candidate) continue;

                if (kv.Value < bestSqr)
                {
                    bestSqr = kv.Value;
                    nearest = candidate;
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
            if (!pv || !pv.IsMine) return;
            if (usedThisRound) return;

            // 현재 지정된 타깃으로 요청
            var target = _outlinedView;
            if (!target)
            {
                if (resultText) resultText.text = "No Target";
                return;
            }

            // 호스트에게 요청: 내 루트 ViewID와 radius만 넘겨 타깃 검증/색상조회
            var myRoot = FindLocalRootView();
            if (!myRoot)
            {
                Debug.LogWarning("[DetectiveAbility] Local root not found.");
                return;
            }

            // 호스트는 별도 탐색 로직을 쓰지만, 여기선 선택된 대상의 확인용으로 충분
            photonView.RPC(nameof(RPC_RequestInspectNearest), RpcTarget.MasterClient, myRoot.ViewID, aimRayLength);
        }

        private PhotonView FindLocalRootView()
        {
            PhotonView localRoot = null;
            foreach (var v in GameObject.FindObjectsOfType<PhotonView>())
            {
                if (v && v.IsMine) { localRoot = v; break; }
            }
            return localRoot;
        }

        [PunRPC]
        private void RPC_RequestInspectNearest(int requesterRootViewID, float radius, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var requester = PhotonView.Find(requesterRootViewID);
            if (!requester)
            {
                Debug.LogWarning("[DetectiveAbility] Host: requester root not found.");
                return;
            }

            // 가장 가까운 타깃은 기존 구현 그대로 사용(신뢰성)
            PhotonView nearest = null;
            float nearestSqr = float.MaxValue;
            Vector3 origin = requester.transform.position;

            var allViews = GameObject.FindObjectsOfType<PhotonView>();
            var myOwner = requester.Owner;

            foreach (var v in allViews)
            {
                if (!v || v.ViewID <= 0) continue;
                if (v.Owner == null) continue;
                if (v.Owner == myOwner) continue; // 자기 자신 제외
                float sqr = (v.transform.position - origin).sqrMagnitude;
                if (sqr < nearestSqr && sqr <= radius * radius)
                {
                    nearestSqr = sqr;
                    nearest = v;
                }
            }

            if (!nearest)
            {
                photonView.RPC(nameof(RPC_ReceiveInspectResult), info.Sender, -1, -1);
                return;
            }

            if (!GameDataManager.Instance.TryGetUIDByViewID(nearest.ViewID, out string targetUID) ||
                !GameDataManager.Instance.TryGetPrivatePlayerData(targetUID, out var priv))
            {
                photonView.RPC(nameof(RPC_ReceiveInspectResult), info.Sender, nearest.ViewID, -1);
                return;
            }

            int colorId = priv.identityColorId;
            photonView.RPC(nameof(RPC_ReceiveInspectResult), info.Sender, nearest.ViewID, colorId);
        }

        [PunRPC]
        private void RPC_ReceiveInspectResult(int targetViewID, int colorId)
        {
            if (!pv || !pv.IsMine) return;

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
