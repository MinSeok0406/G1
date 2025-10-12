using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 타깃팅/하이라이트/틱을 공통 제공하는 베이스.
    /// 파생 클래스는 버튼 로직/쿨다운 UI만 구현하면 됨.
    /// </summary>
    public abstract class TargetingAbilityBase : MonoBehaviourPun
    {
        [Header("Targeting")]
        [SerializeField] protected Material outlineMaterial;
        [SerializeField, Min(0.1f)] protected float aimRadius = 1f;
        [SerializeField] protected LayerMask aimMask2D;
        [SerializeField, Min(0.05f)] protected float aimTickInterval = 0.1f;
        [SerializeField, Min(0.05f)] protected float uiTickInterval = 0.1f;

        protected ITargetingStrategy targeting;
        protected IHighlighter highlighter;

        protected PhotonView currentTarget;
        protected Transform origin;

        private float _aimTick;
        private float _uiTick;

        protected virtual void Awake()
        {
            highlighter = new OutlineHighlighter(outlineMaterial);
            targeting = new CircleNearestTargeting2D(aimRadius, aimMask2D);
        }

        protected virtual void OnEnable()
        {
            origin ??= PlayerManager.Instance.GetMyPlayer()?.transform;
            ClearHighlight();
        }

        protected virtual void OnDisable()
        {
            ClearHighlight();
        }

        protected virtual void Update()
        {
            // UI 틱
            _uiTick += Time.unscaledDeltaTime;
            if (_uiTick >= uiTickInterval)
            {
                _uiTick = 0f;
                OnUiTick();
            }

            // 타깃 틱
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

            var nearest = targeting.AcquireNearest(origin);
            SetTarget(nearest);
        }

        protected void SetTarget(PhotonView newTarget)
        {
            if (newTarget == currentTarget) return;

            // 하이라이트 교체
            if (newTarget)
                highlighter.Apply(newTarget);
            else
                highlighter.Clear();

            currentTarget = newTarget;
            OnTargetChanged(newTarget);
        }

        protected void ClearHighlight()
        {
            currentTarget = null;
            highlighter?.Clear();
        }

        /// <summary>쿨다운 등으로 에임 일시 중지할지</summary>
        protected virtual bool ShouldPauseTargeting() => false;

        /// <summary>타깃이 바뀔 때 파생 행동(버튼 enable 등)</summary>
        protected abstract void OnTargetChanged(PhotonView newTarget);

        /// <summary>UI 틱마다 파생 행동(버튼 interactable 등)</summary>
        protected abstract void OnUiTick();
    }
}
