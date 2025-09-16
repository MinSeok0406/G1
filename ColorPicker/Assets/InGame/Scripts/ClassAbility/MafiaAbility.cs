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

        [Header("ColorPicker")]
        [SerializeField] private GameObject colorPickerButton;

        [Header("Targeting (2D circle)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float aimRadius = 1f;
        [SerializeField] private LayerMask aimMask2D;

        // Legacy 유지
        private Player currentPlayer;
        private Coroutine cooldownRoutine;
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
                uiTick -= 1f;
                UpdateButtonInteractable();
            }

            // 타깃 갱신 (0.1초)
            aimTick += Time.unscaledDeltaTime;
            if (aimTick >= 0.1f)
            {
                aimTick -= 0.1f;
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

        public void StartCooldownUI(float duration)
        {
            // 기존 코루틴이 돌고 있다면 정지
            if (cooldownRoutine != null)
                StopCoroutine(cooldownRoutine);

            float endTime = Time.time + duration;
            cooldownRoutine = StartCoroutine(UpdateCooldownUICoroutine(endTime));
        }

        private void UpdateCooldownUI(float remain)
        {
            if (!cooldownText) return;
            cooldownText.text = remain > 0f ? Mathf.CeilToInt(remain).ToString() : "";
        }

        private IEnumerator UpdateCooldownUICoroutine(float endTime)
        {
            while (true)
            {
                float remain = endTime - Time.time;
                if (remain <= 0f)
                {
                    UpdateCooldownUI(0f);
                    cooldownRoutine = null;
                    yield break;
                }

                UpdateCooldownUI(remain);

                // 1초 단위로만 갱신하고 싶으면
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateButtonInteractable()
        {
            if (!killButton) return;
            killButton.interactable = CanUseAbility();
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

            AbilityManager.Instance.TryRequestKill(targetViewID, myViewID);
        }

        private int ResolveTargetViewID()
        {
            if (_outlinedView && _outlinedView.ViewID > 0)
                return _outlinedView.ViewID;

            return -1;
        }

        public Player GetCurrentPlayer() => currentPlayer;

        #region  ColorPicker

        private void TryRequestColorPick()
        {
            PlayerCardUI playerCard = UIManager.Instance.GetPlayerCard();

            if (!playerCard)
            {
                Debug.Log("Not found PlayerCard Data");
                return;
            }

            int targetActorNum = playerCard.GetActorNum();
            int deductionColor = playerCard.GetColor();

            photonView.RPC(nameof(RPC_TryRequestColorPick), RpcTarget.MasterClient, targetActorNum, deductionColor);
        }

        [PunRPC]
        private void RPC_TryRequestColorPick(int targetActorNum, int deductionColor, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!GameDataManager.Instance.TryGetPrivatePlayerDataByActorId(targetActorNum, out var data))
            {
                Debug.Log($"not found {targetActorNum} data");
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, -1, 0f);
            }

            if (data.identityColorId == deductionColor)
            {
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, 1, 0f);
            }
            else
            {
                photonView.RPC(nameof(RPC_ClientOnColorPickResult), info.Sender, 0, Settings.defaultCooldown);   
            }
        }

        [PunRPC]
        private void RPC_ClientOnColorPickResult(int resultCode, float cooldown)
        {
            //result code : error = -1 / fail = 0 / success = 1
            StartCooldownUI(cooldown);

            
        }   

        #endregion
    }
}
