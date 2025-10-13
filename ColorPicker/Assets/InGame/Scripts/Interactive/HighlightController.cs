using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public enum HighlightState
    {
        OriginalLocked,  // 상호작용 불가: 원본 머티리얼 유지/복구
        DefaultIdle,     // 상호작용 가능(범위 밖): 기본 머티리얼(스프라이트에만 적용)
        ActiveOutline    // 상호작용 가능(범위 내): 아웃라인 머티리얼
    }

    /// <summary>
    /// 단일 컴포넌트로 Sprite/Mesh/SkinnedMesh 하이라이트를 모두 처리.
    /// - SpriteRenderer: sharedMaterial 스왑(기본/아웃라인/원본)
    /// - Mesh/SkinnedMesh: 전 슬롯을 outline 또는 원본으로 복구
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighlightController : MonoBehaviour
    {
        [Header("Renderer Collection")]
        [Tooltip("비워두면 자식까지 자동 수집합니다.")]
        [SerializeField] private List<Renderer> renderers = new();
        [SerializeField] private bool includeInactiveChildren = true;

        [Header("Materials (leave empty to use GameResources fallback)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Material defaultSpriteMaterial; // Sprite 전용 기본 머티리얼(없으면 원본/폴백)

        [Header("Initial State")]
        [SerializeField] private HighlightState initialState = HighlightState.DefaultIdle;

        // Cache
        private readonly Dictionary<Renderer, Material[]> _originals = new();
        private readonly List<SpriteRenderer> _sprites = new();
        private bool _hasMeshLike; // MeshRenderer/SkinnedMeshRenderer 존재 여부
        private bool _collected;

        // State
        private HighlightState _state = HighlightState.DefaultIdle;

        private void Reset()
        {
            AutoCollect();
        }

        private void Awake()
        {
            if (!_collected) AutoCollect();
            CacheOriginals();

            // 초기 상태 적용
            SetHighlightState(initialState, /*force*/ true);
        }

        private void AutoCollect()
        {
            _collected = true;
            renderers ??= new List<Renderer>(8);
            renderers.Clear();
            GetComponentsInChildren(includeInactiveChildren, renderers);

            _sprites.Clear();
            _hasMeshLike = false;

            foreach (var r in renderers)
            {
                if (!r) continue;
                if (r is SpriteRenderer sr) _sprites.Add(sr);
                else _hasMeshLike = true;
            }
        }

        private void CacheOriginals()
        {
            _originals.Clear();
            foreach (var r in renderers)
            {
                if (!r) continue;
                var src = r.sharedMaterials;
                _originals[r] = src != null ? (Material[])src.Clone() : new Material[0];
            }
        }

        private void OnDestroy()
        {
            // 에디터 중 파괴/해제 시에도 원복 시도(런타임 중 제거 케이스 대비)
            TryRestoreAll();
        }

        // ===== Public API =====

        /// <summary>하이라이트 on/off의 하위 호환. (잠금이면 Original 유지)</summary>
        public void SetHighlighted(bool active)
        {
            if (_state == HighlightState.OriginalLocked)
            {
                // 잠금 상태에선 원본 유지
                SetHighlightState(HighlightState.OriginalLocked);
                return;
            }
            SetHighlightState(active ? HighlightState.ActiveOutline : HighlightState.DefaultIdle);
        }

        /// <summary>상호작용 가능 여부를 지정. false면 원본 고정(잠금)으로 전환.</summary>
        public void SetInteractable(bool canInteract)
        {
            SetHighlightState(canInteract ? HighlightState.DefaultIdle : HighlightState.OriginalLocked);
        }

        /// <summary>명시적으로 3상태 지정.</summary>
        public void SetHighlightState(HighlightState state, bool force = false)
        {
            if (!force && _state == state) return;

            var outlineMat = outlineMaterial ?? GameResources.Instance?.outlineMaterial;
            var defaultMat = defaultSpriteMaterial ?? GameResources.Instance?.interactiveMaterial;

            // 1) SpriteRenderer 경로 (스프라이트는 기본머티리얼 개념이 있으므로 3상태 모두 스왑)
            ApplySpriteState(state, outlineMat, defaultMat);

            // 2) Mesh/Skinned 경로 (Default/OriginalLocked는 원본 복구, ActiveOutline은 전 슬롯 outline)
            if (_hasMeshLike)
                ApplyMeshState(state, outlineMat);

            _state = state;
        }

        /// <summary>강제로 모두 원본으로 복구.</summary>
        public void Clear()
        {
            SetHighlightState(HighlightState.OriginalLocked, true);
        }

        // ===== Internal Apply =====

        private void ApplySpriteState(HighlightState state, Material outlineMat, Material defaultMat)
        {
            if (_sprites.Count == 0) return;

            switch (state)
            {
                case HighlightState.OriginalLocked:
                    foreach (var sr in _sprites)
                    {
                        if (!sr) continue;
                        var orig = TryGetOriginal(sr);
                        if (orig != null) sr.sharedMaterial = orig;
                    }
                    break;

                case HighlightState.DefaultIdle:
                    foreach (var sr in _sprites)
                    {
                        if (!sr) continue;
                        // default 지정이 없으면 원본 유지(= 시각적 변화 없음)
                        var target = defaultMat ?? TryGetOriginal(sr);
                        if (target != null) sr.sharedMaterial = target;
                    }
                    break;

                case HighlightState.ActiveOutline:
                    if (!outlineMat) outlineMat = GameResources.Instance?.outlineMaterial;
                    if (!outlineMat) return;
                    foreach (var sr in _sprites)
                    {
                        if (!sr) continue;
                        sr.sharedMaterial = outlineMat;
                    }
                    break;
            }
        }

        private void ApplyMeshState(HighlightState state, Material outlineMat)
        {
            foreach (var r in renderers)
            {
                if (!r || r is SpriteRenderer) continue;

                if (state == HighlightState.ActiveOutline)
                {
                    if (!outlineMat) outlineMat = GameResources.Instance?.outlineMaterial;
                    if (!outlineMat) continue;

                    var slotCount = Mathf.Max(1, r.sharedMaterials?.Length ?? 1);
                    var mats = new Material[slotCount];
                    for (int i = 0; i < slotCount; i++) mats[i] = outlineMat;
                    r.sharedMaterials = mats;
                }
                else
                {
                    // Default/OriginalLocked => 원본 복구
                    if (_originals.TryGetValue(r, out var orig))
                        r.sharedMaterials = orig;
                }
            }
        }

        private Material TryGetOriginal(Renderer r)
        {
            if (!_originals.TryGetValue(r, out var mats)) return null;
            if (mats == null || mats.Length == 0) return null;
            return mats[0];
        }

        private void TryRestoreAll()
        {
            foreach (var kv in _originals)
            {
                var r = kv.Key;
                if (!r) continue;
                r.sharedMaterials = kv.Value;
            }
        }

        // ===== Helper (optional) =====

        public static HighlightController GetOrAdd(GameObject go, bool includeInactive = true)
        {
            if (!go) return null;
            var h = go.GetComponentInChildren<HighlightController>(includeInactive);
            if (!h) h = go.AddComponent<HighlightController>();
            return h;
        }
    }
}
