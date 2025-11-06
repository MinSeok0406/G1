using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 타깃팅/하이라이트/틱 공통. 파생은 버튼/쿨다운 UI만 구현.
    /// </summary>
    public abstract class TargetingAbilityBase : MonoBehaviourPun
    {
        [Header("Targeting")]
        [SerializeField, Min(0.1f)] protected float aimRadius = 5f;
        [SerializeField] protected LayerMask aimMask2D;    // "Player" 레이어만 포함 추천
        [SerializeField, Min(0.05f)] protected float aimTickInterval = 0.1f;
        [SerializeField, Min(0.05f)] protected float uiTickInterval = 0.1f;

        [Header("Highlight Materials (optional)")]
        [SerializeField] private Material outlineMaterialOverride;   // 비우면 GameResources 사용
        [SerializeField] private Material defaultSpriteMaterialOverride;

        protected PhotonView currentTarget;
        protected Transform origin;

        private float _aimTick;
        private float _uiTick;

        // 캐시: 현재 타깃의 HighlightController
        private HighlightController _currentHL;

        protected virtual void Awake() { }

        protected virtual void OnEnable()
        {
            origin ??= PlayerManager.Instance.GetMyPlayer()?.transform;
            ClearHighlight();
        }

        protected virtual void OnDisable() => ClearHighlight();

        protected virtual void Update()
        {
            _uiTick += Time.unscaledDeltaTime;
            if (_uiTick >= uiTickInterval)
            {
                _uiTick = 0f;
                OnUiTick();
            }

            _aimTick += Time.unscaledDeltaTime;
            if (_aimTick >= aimTickInterval)
            {
                _aimTick = 0f;
                AcquireTarget();
            }
        }

        protected void AcquireTarget()
        {
            if (ShouldPauseTargeting())
            {
                SetTarget(null);
                return;
            }

            origin ??= PlayerManager.Instance.GetMyPlayer()?.transform;
            if (!origin)
            {
                SetTarget(null);
                return;
            }

            // === 플레이어만 타겟: Player 레이어만 포함된 aimMask2D 사용 권장
            var cols = Physics2D.OverlapCircleAll(origin.position, aimRadius, aimMask2D);

            PhotonView nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (!c) continue;

                var view = c.GetComponentInParent<PhotonView>();
                if (!view || view.IsMine || view.ViewID <= 0) continue;

                if (!view.CompareTag("Player")) continue;

                float d2 = (view.transform.position - origin.position).sqrMagnitude;
                if (d2 < bestSqr)
                {
                    bestSqr = d2;
                    nearest = view;
                }
            }

            SetTarget(nearest);
        }

        protected void SetTarget(PhotonView newTarget)
        {
            if (newTarget == currentTarget) return;

            // 이전 타깃 하이라이트 원복
            if (_currentHL != null)
                _currentHL.SetHighlightState(HighlightState.OriginalLocked);

            currentTarget = newTarget;

            // 새 타깃 하이라이트 적용
            if (currentTarget)
            {
                _currentHL = currentTarget.GetComponent<HighlightController>()?? HighlightController.GetOrAdd(currentTarget.gameObject);
                if (_currentHL != null)
                {
                    _currentHL.SetInteractable(true);
                    _currentHL.SetHighlightState(HighlightState.ActiveOutline);
                }
            }
            else
            {
                _currentHL = null;
            }

            OnTargetChanged(newTarget);
        }

        protected void ClearHighlight()
        {
            if (_currentHL != null)
                _currentHL.SetHighlightState(HighlightState.OriginalLocked);
            _currentHL = null;
            currentTarget = null;
        }

        /// <summary>쿨다운 등으로 에임 일시 중지할지</summary>
        protected virtual bool ShouldPauseTargeting() => false;

        /// <summary>타깃이 바뀔 때 파생 행동(버튼 enable 등)</summary>
        protected abstract void OnTargetChanged(PhotonView newTarget);

        /// <summary>UI 틱마다 파생 행동(버튼 interactable 등)</summary>
        protected abstract void OnUiTick();
    }
}
