using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 대상의 렌더러 원본 머티리얼을 캐시하고, Outline 머티리얼을 적용/해제한다.
    /// - MeshRenderer / SkinnedMeshRenderer 모두 지원
    /// - 중복 Apply/Clear에 안전
    /// </summary>
    public class OutlineMarker : MonoBehaviour
    {
        private List<Renderer> renderers = new List<Renderer>();

        private readonly Dictionary<Renderer, Material[]> _originals = new();
        private bool _applied;

        private void Awake()
        {
            if (renderers.Count == 0)
            {
                // 자동 수집 (필요시 인스펙터에 수동 지정 가능)
                GetComponentsInChildren(true, renderers);
            }
            CacheOriginals();
        }

        private void OnDestroy()
        {
            // 파괴 전 원복 시도 (씬 종료 시에는 의미 없지만, 런타임 제거에 대비)
            if (_applied) TryRestore();
        }

        private void CacheOriginals()
        {
            _originals.Clear();
            foreach (var r in renderers)
            {
                if (!r) continue;
                // 복제된 배열로 캐시
                _originals[r] = r.sharedMaterials != null ? (Material[])r.sharedMaterials.Clone() : new Material[0];
            }
        }

        public void Apply(Material outline)
        {
            if (!outline) return;
            if (_applied) return;

            foreach (var r in renderers)
            {
                if (!r) continue;

                // 모든 슬롯을 outline으로 채워 깔끔하게 외곽선 전용 셰이더로 교체
                var slotCount = Mathf.Max(1, r.sharedMaterials?.Length ?? 1);
                var arr = new Material[slotCount];
                for (int i = 0; i < slotCount; i++) arr[i] = outline;
                r.sharedMaterials = arr;
            }
            _applied = true;
        }

        public void Clear()
        {
            if (!_applied) return;
            TryRestore();
            _applied = false;
        }

        private void TryRestore()
        {
            foreach (var kv in _originals)
            {
                var r = kv.Key;
                if (!r) continue;
                r.sharedMaterials = kv.Value;
            }
        }

        /// <summary>없으면 붙이고 반환</summary>
        public static OutlineMarker GetOrAdd(GameObject go)
        {
            if (!go) return null;
            var mk = go.GetComponentInChildren<OutlineMarker>(true);
            if (!mk) mk = go.AddComponent<OutlineMarker>();
            return mk;
        }
    }
}
