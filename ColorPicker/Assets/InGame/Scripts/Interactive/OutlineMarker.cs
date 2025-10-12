using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [DisallowMultipleComponent]
    public class OutlineMarker : MonoBehaviour
    {
        [SerializeField] private List<Renderer> renderers = new();

        private readonly Dictionary<Renderer, Material[]> _originals = new();
        private bool _applied;

        private void Reset() => AutoCollect();
        private void Awake()
        {
            if (renderers.Count == 0) AutoCollect();
            CacheOriginals();
        }

        private void OnDestroy()
        {
            if (_applied) TryRestore();
        }

        private void AutoCollect()
        {
            renderers.Clear();
            GetComponentsInChildren(true, renderers);
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

        public void Apply(Material outline)
        {
            if (!outline || _applied) return;

            foreach (var r in renderers)
            {
                if (!r) continue;
                int n = Mathf.Max(1, r.sharedMaterials?.Length ?? 1);
                var mats = new Material[n];
                for (int i = 0; i < n; i++) mats[i] = outline;
                r.sharedMaterials = mats;
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

        public static OutlineMarker GetOrAdd(GameObject go)
        {
            if (!go) return null;
            var mk = go.GetComponentInChildren<OutlineMarker>(true);
            if (!mk) mk = go.AddComponent<OutlineMarker>();
            return mk;
        }
    }
}
